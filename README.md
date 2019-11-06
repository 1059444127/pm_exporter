# pm_mssql_exporter
***
Описание
--
pm_mssql_exporter - поставщик данных [Prometheus](https://prometheus.io/docs/instrumenting/exporters/)
для базы данных MSSQL

Вдохновение черпалось из
  ∙ https://github.com/awaragi/prometheus-mssql-exporter
  ∙ https://github.com/prometheus-net/prometheus-net


Экспортер позволяет получать информацию:
- о подключениях (таблица master.dbo.sysprocesses);
- из встроенные счетчики производительности (таблица sys.dm_os_performance_counters);
- из любых пользовательских запросов.

Экспортер представляет из себя Windows приложение которое может работать в консоли и службой.
Настройка выполняется через App.сonfig файл (pm_mssql_exporter.exe.config).

Для работы в режиме консоли нужно указать параметр console
>\pm_mssql_exporter.exe console

На текущий момент pm_mssql_exporter может работать только Pusher с PushGateway.
"Пассивный" режим со своим веб сервером который может отдать данные Prometheus-у - в планах.

***

**Зачем его делать если есть готовый [prometheus-mssql-exporter](https://github.com/awaragi/prometheus-mssql-exporter)**?
* prometheus-mssql-exporter несколько не из мира .NET, написан на JS, под docker а хотелось что-то в большей степени для Windows инфраструктуры, также 
в моем случае, есть старые системы, на которых из самого свежего работает .NET 4.5 и организационно не до накатывания обновлений и развертывания доккера;
* в нем (на сколько я смог разобраться) фиксированный перечень метрик, от которых на мой взгляд не много толка. Нужны кастомные запросы.

В планах:
* возможность работать с несколькими инстансами (несколько строк подключения);
* возможность собирать под .NET Core;
* возможность работать без PushGateway (встроенный веб сервер)
* размещение в docker;
* свой pm_wmi_exporter и запуск его с pm_mssql_exporter в одной службе. все-таки как правило если на ноде запускается MSSQL, 
  то 99% это Windows машина и нужно следить за её метриками, и желательно без прокладки в виде pushGateway.
***

Текущий статус
-
еще не готово и не тестировано 

Настройка
-
Выполняется в файле pm_mssql_exporter.exe.config в секции pm_mssql_exporter.Properties.Settings.

Строка подключения указывает сервер с которым будет работать программа.
***
JobName и InstanceName попадают в URL строку для prometheus-а
>http://prometheus:XXXX/job/mssql_monitoring/instance/SqlServer1

Если в InstanceName указать Auto, то InstanceName будет браться из строки подключения (connection datasource).

### Режим с PushGateway
* IsEnabledPusher - включить/выключить отправку сообщений
* PushGatewayUri - адрес PushGateway
* PushIntervalSec - интервал отправки в секундах
***


### Настройка метрик
#### master.dbo.sysprocesses 
Данные от подключениях - IsEnabledConnectionInfo, выдается результат выборки
```sql
select db_name(dbid) as [db], loginame as [login], count(*) as [val] 
from master.dbo.sysprocesses 
where Spid <> @@spid 
group by dbid, loginame order by dbid, loginame
```
#### sys.dm_os_performance_counters
Данные из таблицы sys.dm_os_performance_counters настраиваются следующими параметрами:
- IsEnableOsPerfCounters - включает/выключает;
- OsPerformanceCounters - перечень возвращаемых данных.

Основная задумка заключается в том чтоб не указывать фиксированный перечень метрик,
а дать возможность выбрать любую из таблицы  sys.dm_os_performance_counters.

Перечень возвращаемых данных настраивается строкой в которой элементы разделены точкой с запятой:
1) тип счетичка, например gauge
2) название счетчика для prometheus-а
3) значение столбца counter_name таблицы sys.dm_os_performance_counters
4) значение столбца counter_name
5) значение столбца instance_name 
6) часть поля object_name 

Например, если мы хотим выбрать из таблицы sys.dm_os_performance_counters строку

object_name            | counter_name     | instance_name | cntr_value | cntr_type
-----------------------|------------------|---------------|------------|---------
MSSQL$EX2017:Plan Cache| Cache Hit Ratio  | _Total        |        566 | 537003264

то необходимо добавить строку
>gauge; mssql_cache_hit_ratio; Cache Hit Ratio; _Total; Plan Cache
>

Настройки для основного списка метрик в большей части повторяющих то что выдает [prometheus-mssql-exporter](https://github.com/awaragi/prometheus-mssql-exporter)
приведены ниже.

```xml
            <setting name="IsEnableOsPerfCounters" serializeAs="String">
                <value>True</value>
            </setting>
            <setting name="OsPerformanceCounters" serializeAs="Xml">
                <value>
                    <ArrayOfString xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                        <string>gauge  ; mssql_table_lock_escalations_per_sec ; Table Lock Escalations/sec  ;          ;</string>
                        <string>gauge  ; mssql_buffer_cache_hit_ratio         ; Buffer cache hit ratio      ;          ;</string>
                        <string>gauge  ; mssql_average_latch_wait_time_ms     ; Average Latch Wait Time (ms);          ;</string>
                        <string>counter; mssql_activation_errors_total        ; Activation Errors Total     ;          ;</string>
                        <string>gauge  ; mssql_average_wait_time_ms           ; Average Wait Time (ms)      ; _Total   ;</string>
                        <string>gauge  ; mssql_lock_timeouts_per_sec          ; Lock Timeouts/sec           ; _Total   ;</string>
                        <string>gauge  ; mssql_lock_wait_time_ms              ; Lock Wait Time (ms)         ; _Total   ;</string>
                        <string>gauge  ; mssql_number_of_deadlocks_per_sec    ; Number of Deadlocks/sec     ; _Total   ;</string>
                        <string>gauge  ; mssql_cache_hit_ratio                ; Cache Hit Ratio             ; _Total   ;   Plan Cache </string>
                        <string>gauge  ; mssql_transactions_per_sec           ; Transactions/sec            ; _Total   ;</string>
                    </ArrayOfString>
                </value>
            </setting>
```
***

#### Пользовательские метрики
Пользовательские метрики позволяют собирать информацию о работе бизнес-процессов приложения.

Включение/выключение - IsEnableCustomMetrics

Сам запрос описывается XML структурой в App.config
```xml
                        <CustomMetricSett Type="gauge" Name="mssql_transas_log_book_insert"
                            IsEnabled="true">
                            <Help>row count of vtsdb.dbo.LogBook</Help>
                            <Sql>select count(*) as cn from  vtsdb.dbo.logbook</Sql>
                        </CustomMetricSett>
```


Необходимо писать запросы так, чтоб последнее поле было значением метрики, а предыдущие - дополнительный аргумент к Name

Например, в указанном выше запросе всего одно поле 'count' и метрика будет выглядеть следующим образом
```
# HELP row count of vtsdb.dbo.LogBook
# TYPE gauge
mssql_transas_log_book_insert 12345
```

Другой пример
```xml
                        <CustomMetricSett Type="gauge" Name="mssql_vtsdb2_active" IsEnabled="true">
                            <Help>vessel by zone and receipt type</Help>
                            <Sql>
                                select z.RZoneVTS_Prefix, t.ReceiptType_Code, count(*) as cn
                                from vtsdb2.nav.receiptActive as a
                                join vtsdb2.nav.Receipt as r on r.Receipt_GUID = a.Receipt_GUID
                                join vtsdb2.nsi.ReceiptType as t on t.ReceiptType_GUID = r.ReceiptType_GUID
                                join vtsdb2.nsi.ResponsZoneVTS as z on z.ResponsZoneVTS_GUID = a.ResponsZoneVTS_GUID
                                group by z.RZoneVTS_Prefix, t.ReceiptType_Code
                            </Sql>
                        </CustomMetricSett>
```
результат
```
# HELP vessel by zone and receipt type
# TYPE gauge
mssql_vtsdb2_active{RZoneVTS_Prefix="RG",ReceiptType_Code="move"} 1
mssql_vtsdb2_active{RZoneVTS_Prefix="RG",ReceiptType_Code="anchor"} 2
mssql_vtsdb2_active{RZoneVTS_Prefix="RS",ReceiptType_Code="move"} 3
mssql_vtsdb2_active{RZoneVTS_Prefix="MF",ReceiptType_Code="move"} 4
mssql_vtsdb2_active 10
```