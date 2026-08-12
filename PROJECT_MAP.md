# Карта проекта

## Корень репозитория

- `Mikhailov/` — исходники и ресурсы, включаемые в мод.
- `references/` — исходные изображения и концепты; не входит в сборку мода.
- `README.md` — требования, сборка и общая информация.
- `AGENTS.md` — обязательные правила для AI-агентов.
- `update.bat` — обновление локального репозитория через `git pull --ff-only`.

## Мод `Mikhailov`

- `Mikhailov.cs` — точка входа, класс `Mikhailov : Mod`.
- `Mikhailov.csproj` — проект .NET 8 и подключение `tMLMod.targets`.
- `build.txt` — метаданные tModLoader и текущая версия мода.
- `description.txt` — описание мода.
- `icon.png` — иконка 80×80 для интерфейса tModLoader.
- `icon_workshop.png` — обложка 512×512 для Steam Workshop.

## Сборка и версия

```powershell
dotnet build .\Mikhailov\Mikhailov.csproj
```

Если tModLoader установлен нестандартно, передать `-p:TMLInstallDir="<путь>"`.
Версия имеет формат `MAJOR.MINOR.PATCH` и хранится в `Mikhailov/build.txt`.
История изменений отслеживается через Git.

Обновляйте этот файл при добавлении новых подсистем или изменении структуры каталогов.
