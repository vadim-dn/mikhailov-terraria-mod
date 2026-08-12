# Михайлов

Мод для Terraria 1.4.4.9 на базе tModLoader 2026.6.3.4.

## Требования

- tModLoader 2026.6.3.4
- .NET 8 SDK

По умолчанию проект ищет tModLoader в стандартной папке Steam для Windows:
`C:\Program Files (x86)\Steam\steamapps\common\tModLoader`.

Для другого расположения передайте путь при сборке:

```powershell
dotnet build .\Mikhailov\Mikhailov.csproj -p:TMLInstallDir="D:\Games\tModLoader"
```

## Сборка

```powershell
dotnet build .\Mikhailov\Mikhailov.csproj
```

Исходники находятся в каталоге `Mikhailov`, имя которого задаёт внутренний
идентификатор мода. Готовый файл мода устанавливается tModLoader в каталог
локальных модов.

## Версионирование

Проект использует семантические версии `MAJOR.MINOR.PATCH`. Актуальная версия
задаётся в `Mikhailov/build.txt`. История изменений хранится в Git, а релизы
отмечаются тегами вида `v0.1.0`.
