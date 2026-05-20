# NotesSystem — Консольная система заметок и мониторинга

Консольное приложение для ведения служебных заметок, просмотра логов безопасности и мониторинга нагрузки устройств в ИТ-инфраструктуре VPN-сервиса.

Проект разработан на C# с использованием .NET Framework 4.7.2.  
В качестве базы данных используется PostgreSQL.

---

## Содержание

- [Требования](#требования)
- [Установка](#установка)
- [Настройка базы данных](#настройка-базы-данных)
- [Запуск](#запуск)
- [Команды](#команды)
- [Роли пользователей](#роли-пользователей)
- [Примеры использования](#примеры-использования)
- [Автообновление](#автообновление)
- [Тестирование](#тестирование)
- [Структура проекта](#структура-проекта)

---

## Требования

- Windows 10 / 11
- Visual Studio
- .NET Framework 4.7.2
- PostgreSQL
- pgAdmin
- Git
- GitHub

---

## Назначение системы

NotesSystem предназначена для малого бизнеса, предоставляющего VPN-сервис.

Система позволяет:

- создавать служебные заметки на устройствах ИТ-инфраструктуры;
- выполнять авторизацию пользователей;
- разграничивать доступ по ролям;
- просматривать логи безопасности;
- получать статистику CPU, RAM и HDD;
- запускать агент мониторинга;
- проверять обновления через GitHub.

---

## Установка

1. Клонировать репозиторий:

```bash
git clone https://github.com/Vitalya1357/NotesSystem.git
cd NotesSystem
```

2. Открыть решение в Visual Studio:

```text
NotesSystem.sln
```

3. Восстановить NuGet-пакеты.

4. Собрать решение:

```text
Build → Build Solution
```

или:

```text
Ctrl + Shift + B
```

После сборки в общей папке должны находиться исполняемые файлы:

```text
NotesCli.exe
NotesInstaller.exe
NotesWatcher.exe
NotesShared.dll
```

---

## Настройка базы данных

Используется база данных PostgreSQL:

```text
NotesSystemDB
```

Основные таблицы:

```text
roles
users
notes
devices
system_metrics
security_logs
app_versions
```

Основные хранимые функции:

```text
login_user
add_note
edit_note
delete_note
restore_note
```

---

## Роли в базе данных

В таблице `roles` используются три роли приложения:

```text
admin
user
statistic
```

Пример добавления ролей:

```sql
INSERT INTO roles(name)
VALUES 
('admin'),
('user'),
('statistic')
ON CONFLICT (name) DO NOTHING;
```

---

## Тестовые пользователи

В системе используются тестовые пользователи:

| Логин | Пароль | Роль |
|---|---|---|
| `admin` | `admin123` | `admin` |
| `user1` | `admin123` | `user` |
| `stat` | `admin123` | `statistic` |

Пароли хранятся в базе данных в виде SHA-256-хеша.

Хеш пароля `admin123`:

```text
240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
```

---

## Подключение к базе данных

Строка подключения хранится в проекте `NotesShared`:

```text
NotesShared/Config/AppConfig.cs
```

Пример:

```csharp
public static string ConnectionString =
    "Host=localhost;Port=5432;Database=NotesSystemDB;Username=vitalya;Password=your_password";
```

Перед запуском приложения необходимо указать свой пароль от PostgreSQL.

---

## Запуск

### Запуск установщика

```bash
NotesInstaller.exe
```

Установщик создаёт рабочие папки:

```text
C:\NotesSystem
C:\NotesSystem\logs
C:\NotesSystem\config
```

Также создаётся файл конфигурации:

```text
C:\NotesSystem\config\config.json
```

---

### Запуск основного клиента

```bash
NotesCli.exe
```

При запуске `NotesCli.exe` автоматически открывается второе консольное окно с агентом мониторинга:

```text
NotesWatcher.exe
```

Таким образом пользователь видит две консоли:

```text
1. NotesCli — основная консоль управления
2. NotesWatcher — агент мониторинга
```

---

### Запуск агента мониторинга отдельно

```bash
NotesWatcher.exe
```

Агент каждые 30 секунд собирает статистику устройства и сохраняет её в PostgreSQL.

Собираются следующие показатели:

```text
CPU
RAM
HDD
DeviceName
IP Address
DateTime
```

---

## Команды

### Общие команды

| Команда | Описание |
|---|---|
| `--help` | Показать список команд |
| `--version` | Показать текущую версию приложения |
| `exit` | Закрыть приложение |

---

### Авторизация

| Команда | Описание |
|---|---|
| `--login <username> <password>` | Войти в систему |
| `--logout` | Выйти из системы |
| `--myrole` | Показать текущую роль |

Примеры:

```bash
--login admin admin123
--login user1 admin123
--login stat admin123
```

---

### Заметки

| Команда | Описание |
|---|---|
| `--addNewNote "текст"` | Создать новую заметку |
| `--listNotes` | Показать список своих заметок |
| `--editNote <id> "новый текст"` | Изменить заметку |
| `--deleteNote <id>` | Удалить заметку |
| `--restoreNote <id>` | Восстановить удалённую заметку |

Примеры:

```bash
--addNewNote "Проверить сервер БД"
--listNotes
--editNote 1 "Проверить сервер БД после обновления"
--deleteNote 1
--restoreNote 1
```

---

### Логи безопасности

| Команда | Описание |
|---|---|
| `--securityLogs list` | Показать последние 50 логов безопасности |

Команда доступна только пользователю с ролью:

```text
admin
```

Пример:

```bash
--securityLogs list
```

---

### Статистика системы

| Команда | Описание |
|---|---|
| `--systemStats local` | Показать текущую статистику устройства |
| `--systemStats history` | Показать последние записи статистики из базы данных |

Команды доступны ролям:

```text
admin
statistic
```

Примеры:

```bash
--systemStats local
--systemStats history
```

---

### Обновление

| Команда | Описание |
|---|---|
| `--checkUpdate` | Проверить наличие обновлений через GitHub |
| `--update` | Скачать обновление |
| `--version` | Показать текущую версию |

Пример:

```bash
--checkUpdate
```

---

## Роли пользователей

| Роль | Значение | Доступ |
|---|---|---|
| `admin` | Администратор | Все функции системы |
| `user` | Пользователь | Работа со своими заметками |
| `statistic` | Пользователь статистики | Просмотр статистики CPU/RAM/HDD |

---

## Права доступа

| Функция | admin | user | statistic |
|---|---:|---:|---:|
| Авторизация | Да | Да | Да |
| Создание заметок | Да | Да | Нет |
| Просмотр заметок | Да | Да | Нет |
| Редактирование заметок | Да | Да | Нет |
| Удаление заметок | Да | Да | Нет |
| Восстановление заметок | Да | Да | Нет |
| Просмотр логов безопасности | Да | Нет | Нет |
| Просмотр статистики | Да | Нет | Да |
| Проверка обновлений | Да | Да | Да |

---

## Примеры использования

### Вход администратора

```bash
--login admin admin123
```

Результат:

```text
Добро пожаловать, admin!
Ваша роль: admin
```

---

### Создание заметки

```bash
--addNewNote "Проверить VPN-шлюз gateway-01"
```

Результат:

```text
Заметка создана. ID: 1
```

---

### Просмотр заметок

```bash
--listNotes
```

Пример результата:

```text
[1] 13.05.2026 12:30:00 | активна
Проверить VPN-шлюз gateway-01
```

---

### Просмотр логов безопасности

```bash
--securityLogs list
```

Пример результата:

```text
Логи безопасности:
[1] 13.05.2026 12:30:00 | UserId: 1 | NOTE_CREATED | Создана заметка #1
```

---

### Просмотр локальной статистики

```bash
--systemStats local
```

Пример результата:

```text
Статистика устройства:
Устройство: DESKTOP-01
IP: 192.168.0.10
CPU: 15.2%
RAM: 63.4%
HDD: 48.1%
Дата: 13.05.2026 12:35:00
```

---

### Просмотр истории статистики

```bash
--systemStats history
```

Пример результата:

```text
Последние записи статистики:
13.05.2026 12:35:00 | DESKTOP-01 | IP: 192.168.0.10 | CPU: 15.2% | RAM: 63.4% | HDD: 48.1%
```

---

## Автообновление

Для проверки обновлений используется файл:

```text
releases/version.json
```

Пример содержимого:

```json
{
  "version": "1.0.0",
  "downloadUrl": "https://github.com/Vitalya1357/NotesSystem/releases/download/v1.0.0/NotesSystem.zip"
}
```

Ссылка на файл обновления хранится в:

```text
NotesShared/Config/AppConfig.cs
```

Пример:

```csharp
public static string UpdateInfoUrl =
    "https://raw.githubusercontent.com/Vitalya1357/NotesSystem/develop/releases/version.json";
```

Команда проверки:

```bash
--checkUpdate
```

Если версия на GitHub новее локальной, приложение сообщает о доступном обновлении.

---

## GitHub и ветки

В проекте используются две ветки:

```text
main      # стабильная версия
develop   # ветка разработки
```

Пример стандартной работы:

```bash
git checkout develop
git add .
git commit -m "feat: add new feature"
git push
```

После завершения разработки изменения переносятся в `main`:

```bash
git checkout main
git merge develop
git push
```

Финальная версия отмечается тегом:

```bash
git tag v1.0.0
git push origin v1.0.0
```

---

## Тестирование

Для тестирования создан отдельный проект:

```text
NotesUnitTests
```

Используется MSTest.

Тесты находятся в папке:

```text
NotesUnitTests/TestCollection
```

Реализованы тесты:

```text
UnitTest_AuthService
UnitTest_DatabaseConnection
UnitTest_NoteService
UnitTest_UpdateService
```

Проверяются:

- корректность SHA-256-хеширования пароля;
- успешная авторизация администратора;
- запрет входа с неверным паролем;
- авторизация пользователя;
- авторизация пользователя статистики;
- проверка ролей;
- получение статистики CPU/RAM/HDD.

Запуск тестов:

```text
Test → Run All Tests
```

или:

```text
Тест → Выполнить все тесты
```

---

## Структура проекта

```text
NotesSystem/
├── NotesCli/
│   └── Program.cs
├── NotesInstaller/
│   └── Program.cs
├── NotesWatcher/
│   └── Program.cs
├── NotesShared/
│   ├── Config/
│   │   └── AppConfig.cs
│   ├── Database/
│   │   └── DatabaseConnection.cs
│   ├── Models/
│   │   ├── Note.cs
│   │   ├── User.cs
│   │   ├── SecurityLog.cs
│   │   ├── SystemMetric.cs
│   │   └── UpdateInfo.cs
│   ├── Services/
│   │   ├── AuthService.cs
│   │   ├── NoteService.cs
│   │   ├── SecurityLogService.cs
│   │   ├── SystemMetricService.cs
│   │   └── UpdateService.cs
│   └── Utils/
│       └── PasswordHasher.cs
├── NotesUnitTests/
│   └── TestCollection/
│       ├── UnitTest_DatabaseConnection.cs
│       ├── UnitTest_AuthService.cs
│       ├── UnitTest_NoteService.cs
│       └── UnitTest_UpdateService.cs
├── releases/
│   └── version.json
├── README.md
└── NotesSystem.sln
```

---

## Описание компонентов

### NotesCli

Основной консольный клиент.

Функции:

- авторизация;
- работа с заметками;
- просмотр логов;
- просмотр статистики;
- проверка обновлений;
- автоматический запуск `NotesWatcher`.

---

### NotesInstaller

Установщик приложения.

Функции:

- создание папки `C:\NotesSystem`;
- создание папки логов;
- создание папки конфигурации;
- создание `config.json`;
- проверка подключения к PostgreSQL.

---

### NotesWatcher

Агент мониторинга.

Функции:

- сбор статистики CPU;
- сбор статистики RAM;
- сбор статистики HDD;
- получение имени устройства;
- получение IP-адреса;
- запись статистики в PostgreSQL.

---

### NotesShared

Общая библиотека классов.

Содержит:

- модели;
- сервисы;
- подключение к базе данных;
- хеширование паролей;
- бизнес-логику приложения.

---

### NotesUnitTests

Проект модульных тестов.

Используется для проверки основных функций системы.

---

## Версия приложения

Текущая версия:

```text
1.0.0
```

---

## Лицензия

Учебный проект. Курсовая работа по дисциплине, связанной с разработкой, тестированием и отладкой программного обеспечения.

---

