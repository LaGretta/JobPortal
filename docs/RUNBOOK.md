# JobPortal — запуск, оновлення та перевірка

Інструкція для бекенду. Усе, що треба, щоб підняти проект з нуля, задеплоїти зміни й переконатись, що після публікації він живий.

---

## 1. Що де працює

| Що | Де | Назва |
|---|---|---|
| API | Azure App Service, план F1, Sweden Central | `jobportal-pl-api` |
| База | Azure SQL, рівень Basic | сервер `jobportal-sql-sasha2626`, база `JobPortal` |
| Файли резюме | Azure Blob Storage | акаунт `jobportalcv`, контейнер `resumes` |
| Деплой | GitHub Actions | `.github/workflows/deploy.yml` |
| Група ресурсів | Azure, Poland Central | `jobportal-rg` |

Публічна адреса: `https://jobportal-pl-api.azurewebsites.net`

---

## 2. Запуск локально

### Потрібно

- .NET SDK 10
- SQL Server або LocalDB (`(localdb)\MSSQLLocalDB` ставиться разом з Visual Studio; для Rider — SQL Server Express)
- Azure CLI — лише якщо працюєш з ресурсами в Azure
- `sqlcmd` — лише щоб дивитись у базу руками: `winget install Microsoft.Sqlcmd`

### Кроки

```powershell
git clone https://github.com/LaGretta/JobPortal.git
cd JobPortal
```

Створи `JobPortal.Api/appsettings.Development.json` за зразком `appsettings.Example.json` і заповни значення. **Цей файл у `.gitignore` — у репозиторій він не потрапляє й не повинен.**

```powershell
dotnet restore
dotnet build
```

Накатити міграції на локальну базу:

```powershell
dotnet ef database update -p JobPortal.Infrastructure -s JobPortal.Api
```

Запустити:

```powershell
dotnet run --project JobPortal.Api
```

Swagger: http://localhost:5132/swagger/index.html

---

## 3. Конфігурація

Жодного секрету в репозиторії. Локально вони живуть у `appsettings.Development.json`, на сервері — в налаштуваннях App Service. Структура однакова, відрізняється лише запис вкладеності: у JSON через об'єкти, у налаштуваннях App Service через `__` (два підчеркування).

| Налаштування | Що це | Обов'язкове |
|---|---|---|
| `ConnectionStrings:Default` | рядок підключення до MS SQL | так |
| `Jwt:Key` | ключ підпису токенів, **не коротше 32 байт** | так |
| `Jwt:Issuer`, `Jwt:Audience` | хто видав і для кого токен | так |
| `Jwt:AccessTokenMinutes` | 15 | так |
| `Jwt:RefreshTokenDays` | 7 | так |
| `BlobStorage:ConnectionString` | повний рядок підключення до сховища | так |
| `BlobStorage:ContainerName` | `resumes` | так |
| `Email:Host`, `Email:Port` | `smtp.gmail.com`, `587` | для відновлення пароля |
| `Email:User`, `Email:Password` | пошта і **App Password** Gmail, не звичайний пароль | для відновлення пароля |
| `Email:FromName`, `Email:FromAddress` | від кого приходить лист | для відновлення пароля |
| `Email:ResetUrlBase` | адреса сторінки фронту для скидання пароля | для відновлення пароля |
| `Cors:Origins:0`, `:1`, ... | дозволені origin фронтенду | так |
| `Admin:Email`, `Admin:Password` | адмін, якого створює застосунок при старті | для адмінки |
| `Admin:FirstName`, `Admin:LastName` | імʼя адміна | ні |

### Чому застосунок падає при старті без `BlobStorage:ConnectionString`

Так зроблено навмисно. `AddInfrastructure` перевіряє, що рядок починається на `DefaultEndpointsProtocol=`, і якщо ні — кидає виняток з текстом про це. Без цієї перевірки застосунок піднімався б, а кожен запит до профілю повертав би `500 An unexpected error occurred`, і справжня причина лежала б у логах за сотнею рядків стеку.

Правило: настройка, без якої застосунок не працює, перевіряється на старті.

### Переглянути налаштування на сервері

```powershell
az webapp config appsettings list -g jobportal-rg -n jobportal-pl-api -o table
```

Команда показує назви й значення — тобто й секрети. Не запускай її там, де хтось дивиться в екран.

### Додати origin фронтенду

Індекси йдуть по порядку, без пропусків:

```powershell
az webapp config appsettings set -g jobportal-rg -n jobportal-pl-api --settings "Cors__Origins__2=https://нова-адреса"
```

Origin пишеться **без слеша в кінці** і без шляху.

---

## 4. Сховище файлів

Створюється один раз. Якщо колись доведеться робити з нуля:

```powershell
az storage account check-name --name jobportalcv
```

```powershell
az storage account create -g jobportal-rg -n jobportalcv -l swedencentral --sku Standard_LRS --kind StorageV2 --allow-blob-public-access false
```

`--allow-blob-public-access false` — основа вимоги «резюме бачить тільно роботодавець, якому подались». Файли недоступні за прямим посиланням; віддає їх API після перевірки прав.

Регіон той самий, що в App Service: інакше кожне скачування було б платним трафіком між регіонами.

```powershell
$cs = az storage account show-connection-string -g jobportal-rg -n jobportalcv --query connectionString -o tsv
az storage container create --name resumes --connection-string $cs
```

Прописати в App Service:

```powershell
az webapp config appsettings set -g jobportal-rg -n jobportal-pl-api --settings "BlobStorage__ConnectionString=$cs" "BlobStorage__ContainerName=resumes"
```

### Подивитись, що лежить у сховищі

```powershell
az storage blob list -c resumes --connection-string $cs --query "[].name" -o tsv
```

Шляхи двох видів:

- `{userId}/{guid}.pdf` — резюме в профілі кандидата
- `applications/{candidateId}/{guid}.pdf` — копія резюме на момент відгуку

Копії не видаляються разом з резюме в профілі: саме тому роботодавець бачить те, що йому надіслали, навіть якщо кандидат потім замінив або видалив CV.

### Перевипустити ключ сховища

```powershell
az storage account keys renew -g jobportal-rg -n jobportalcv --key primary
$cs = az storage account show-connection-string -g jobportal-rg -n jobportalcv --query connectionString -o tsv
az webapp config appsettings set -g jobportal-rg -n jobportal-pl-api --settings "BlobStorage__ConnectionString=$cs"
```

Після цього обов'язково онови `appsettings.Development.json` локально — старий ключ перестає діяти одразу.

---

## 5. Міграції

### Додати нову

```powershell
dotnet ef migrations add НазваЗміни -p JobPortal.Infrastructure -s JobPortal.Api
```

```powershell
dotnet ef database update -p JobPortal.Infrastructure -s JobPortal.Api
```

Правило: усі зміни схеми за один захід і **одна** міграція. П'ять дрібних міграцій за вечір — це п'ять шансів, що четверта суперечить першій.

### Як вони застосовуються на сервері

**Автоматично, пайплайном, перед деплоєм.** Руками на Azure нічого робити не треба.

Крок `Apply migrations` у `deploy.yml` робить так:

1. дізнається IP раннера GitHub і додає тимчасове правило у фаєрвол Azure SQL з назвою `gh-<номер прогону>`
2. читає рядок підключення з налаштувань App Service — тобто пароль бази не стає ще одним секретом у GitHub
3. маскує його через `::add-mask::`, щоб він не вилетів у лог
4. виконує `dotnet ef database update`
5. видаляє правило фаєрволу, причому в `if: always()` — інакше кожен невдалий прогон лишав би відкриту адресу

Міграції йдуть **до** деплою: якщо навпаки, новий код прийде на базу, якої він ще не знає, і впаде на першому ж запиті.

Крок передає фіктивне `BlobStorage__ConnectionString` правильної форми. `dotnet ef` запускає `Program.cs` цілком, щоб дістати `DbContext`, тому спрацьовує й перевірка з розділу 3 — а справжній ключ сховища пайплайну не потрібен.

### Накатити вручну, якщо треба

```powershell
$conn = az webapp config appsettings list -g jobportal-rg -n jobportal-pl-api --query "[?name=='ConnectionStrings__Default'].value" -o tsv
dotnet ef database update -p JobPortal.Infrastructure -s JobPortal.Api --connection $conn
```

Потрібне правило у фаєрволі на твою адресу. Вона змінюється, тому зазвичай треба оновити:

```powershell
$ip = (Invoke-RestMethod https://ipv4.icanhazip.com).Trim()
az sql server firewall-rule update -g jobportal-rg -s jobportal-sql-sasha2626 -n AllowMyIp --start-ip-address $ip --end-ip-address $ip
```

---

## 6. Оновлення на сервері

```powershell
git push
```

Пуш у `main` запускає `deploy.yml`: збірка, zip, вхід в Azure, міграції, деплой, закриття фаєрволу. Нічого натискати не треба.

Деплой автентифікується сервіс-принципалом — секрет `AZURE_CREDENTIALS` у репозиторії, JSON з полями `clientId`, `clientSecret`, `subscriptionId`, `tenantId`.

Публікаційний профіль **не використовується**, і базова аутентифікація `scm`/`ftp` на App Service відключена:

```powershell
az resource show -g jobportal-rg --name scm --namespace Microsoft.Web --resource-type basicPublishingCredentialsPolicies --parent sites/jobportal-pl-api --query "properties.allow"
```

Має бути `false`.

### Перевипустити секрет деплою

```powershell
$sub = az account show --query id -o tsv
$appId = az ad sp list --display-name jobportal-deploy-2026-11 --query "[0].appId" -o tsv
$reset = az ad sp credential reset --id $appId | ConvertFrom-Json

$creds = [ordered]@{
    clientId       = $reset.appId
    clientSecret   = $reset.password
    subscriptionId = $sub
    tenantId       = $reset.tenant
} | ConvertTo-Json

[System.IO.File]::WriteAllText("$env:TEMP\sp.json", $creds, (New-Object System.Text.UTF8Encoding $false))
notepad $env:TEMP\sp.json
```

Вставити вміст у секрет `AZURE_CREDENTIALS` на GitHub, потім `Remove-Item $env:TEMP\sp.json`.

`az ad sp create-for-rbac --sdk-auth` у нових версіях CLI **не працює** — віддає JSON з полями `appId`, `password`, `tenant`, а `azure/login` чекає інші назви. Тому JSON складаємо руками, як вище.

---

## 7. Адміністратор

Адміном **неможливо зареєструватись** — у DTO реєстрації немає поля роли, а ендпоінтів два, і кожен жорстко задає свою роль.

Адмін створюється застосунком при старті з налаштувань `Admin:Email` і `Admin:Password`, якщо користувача з такою поштою ще немає. Два свідомих обмеження:

- якщо користувач з такою поштою вже існує й він **не** адмін — роль не підвищується, лише запис у лог. Інакше конфіг став би способом видати комусь права, вписавши його пошту.
- пароль наявному адміну **не перезаписується**. Зміна налаштування не повинна тихо скидати чийсь пароль.

### Змінити пароль адміна

Через друге обмеження просто поміняти налаштування недостатньо. Процедура:

```powershell
az webapp config appsettings set -g jobportal-rg -n jobportal-pl-api --settings 'Admin__Password=новий_пароль'
```

**Одинарні лапки обов'язкові.** У подвійних PowerShell підставить значення змінної на місце `$...` — і в сховище поїде обрізаний пароль, а логін потім не працюватиме.

Далі прибрати наявного адміна й перезапустити застосунок, щоб сіяч створив його заново:

```powershell
$conn = az webapp config appsettings list -g jobportal-rg -n jobportal-pl-api --query "[?name=='ConnectionStrings__Default'].value" -o tsv
$dbpw = ($conn -split ';' | Where-Object { $_ -like 'Password=*' }) -replace '^Password=',''

sqlcmd -S jobportal-sql-sasha2626.database.windows.net -d JobPortal -U jpadmin -P $dbpw -Q "DELETE FROM Users WHERE Role = 2;"

az webapp restart -g jobportal-rg -n jobportal-pl-api
```

Перевірити, що зʼявився:

```powershell
sqlcmd -S jobportal-sql-sasha2626.database.windows.net -d JobPortal -U jpadmin -P $dbpw -Q "SELECT Email, Role, CreatedAt FROM Users WHERE Role = 2;"
```

---

## 8. Тести

```powershell
dotnet test
```

53 інтеграційних тести. Піднімається застосунок цілком — справжні контролери, справжній JWT, справжній MS SQL. Підмінені лише сховище файлів і пошта.

Що варто знати:

- **потрібен LocalDB.** Для кожного прогону створюється база `JobPortalTests_<guid>`, на неї накатуються міграції, і в кінці вона видаляється. Адреса сервера зашита в `ApiFactory`; якщо в тебе інший — міняти там.
- **SQLite не підійде.** Перевірка на повторний відгук тримається на унікальному індексі MS SQL і кодах помилок 2601/2627, які `UnitOfWork` перетворює в `ConflictException`. SQLite віддав би свій код, і тест проходив би не через ту гілку коду, що в проді.
- **налаштування підставляються змінними середовища**, не через `ConfigureAppConfiguration`. `Program.cs` читає конфіг під час свого виконання, а делегати `ConfigureAppConfiguration` застосовуються пізніше — на той момент конфігу ще немає.
- сховище файлів підмінене на `FakeFileStorage` у памʼяті. Тести на прибирання сиріт рахують різницю в кількості файлів до й після, а не абсолютне число: сховище одне на всю колекцію.

---

## 9. Перевірка після публікації

Швидкий прохід, 5 хвилин. Робити після кожного деплою, який чіпав щось крім текстів.

### 1. API піднявся

Відкрити https://jobportal-pl-api.azurewebsites.net/swagger/index.html — Swagger мусить промалюватись.

### 2. Міграції застосувались

В Actions крок `Apply migrations` зелений. Або вручну:

```powershell
sqlcmd -S jobportal-sql-sasha2626.database.windows.net -d JobPortal -U jpadmin -P $dbpw -Q "SELECT TOP 3 MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC;"
```

Остання міграція мусить збігатись з останньою в `JobPortal.Infrastructure/Migrations`.

### 3. Головний сценарій

Через Swagger, по порядку:

1. `POST /api/Auth/register/employer` → `200`
2. вставити `accessToken` в **Authorize**
3. `POST /api/Vacancy` → `200`, далі `POST /api/Vacancy/{id}/publish` → `200`
4. `POST /api/Auth/register/candidate` → `200`, перевставити токен
5. `POST /api/Profile/my/cv` з PDF → `200`, `hasCv: true`
6. `POST /api/Application` з id вакансії → `200`
7. перевставити токен роботодавця, `GET /api/Application/{id}/cv` → файл скачується

Крок 5 — найважливіший: він єдиний перевіряє, що сховище файлів налаштоване правильно. Якщо `BlobStorage:ConnectionString` поламаний, упадуть усі ендпоінти профілю.

### 4. Адмінка

`POST /api/Auth/login` адміном → `user.role` має бути `2`. Далі `GET /api/Admin/users` → список.

### 5. Права

Під токеном кандидата `GET /api/Admin/users` мусить дати `403`, без токена — `401`.

### 6. CORS

З консолі браузера на сторінці фронтенду:

```js
await fetch('https://jobportal-pl-api.azurewebsites.net/api/Vacancy/search?pageSize=1')
```

Має пройти. Якщо браузер блокує — origin фронту не в списку (розділ 3).

### 7. Прибрати за собою

Тестові акаунти з продакшн-бази:

```powershell
$sql = @"
DELETE FROM JobApplications
WHERE CandidateId IN (SELECT Id FROM Users WHERE Email LIKE '%@example.com')
   OR VacancyId IN (SELECT v.Id FROM Vacancies v JOIN Companies c ON v.CompanyId = c.Id
                    WHERE c.OwnerId IN (SELECT Id FROM Users WHERE Email LIKE '%@example.com'));

DELETE FROM FavoriteVacancies
WHERE UserId IN (SELECT Id FROM Users WHERE Email LIKE '%@example.com')
   OR VacancyId IN (SELECT v.Id FROM Vacancies v JOIN Companies c ON v.CompanyId = c.Id
                    WHERE c.OwnerId IN (SELECT Id FROM Users WHERE Email LIKE '%@example.com'));

DELETE FROM Users WHERE Email LIKE '%@example.com';
"@

sqlcmd -S jobportal-sql-sasha2626.database.windows.net -d JobPortal -U jpadmin -P $dbpw -Q $sql
```

Порядок не випадковий: у конфігурації `JobApplication → Candidate` і `FavoriteVacancy → Vacancy` стоїть `Restrict`, тому база не дасть видалити користувача, поки на нього є посилання. Решта піде каскадом.

---

## 10. Що ламалось раніше і як лікувати

| Симптом | Причина | Лікування |
|---|---|---|
| `FormatException: No valid combination of account information found` на старті | у `BlobStorage:ConnectionString` лежить лише ключ, а не весь рядок | значення мусить починатись на `DefaultEndpointsProtocol=` і закінчуватись на `core.windows.net` |
| `Client with IP address ... is not allowed to access the server` | змінилась твоя IP | оновити правило `AllowMyIp`, розділ 5 |
| `Login failed for user 'jpadmin'` | пароль у рядку підключення не той | взяти рядок з налаштувань App Service, не набирати руками |
| Логін адміном дає `401`, хоч пароль правильний | PowerShell зʼїв частину пароля при записі налаштування | ставити в **одинарних** лапках, розділ 7 |
| Деплой: `Not all parameters are provided in 'creds'` | JSON секрету не того формату | скласти руками, розділ 6 |
| Деплой: Kudu віддає `400` на zip | архів зроблений Windows-засобами, у шляхах зворотні слеші | пакувати `tar -a -c -f ..\publish.zip *`, у пайплайні це вже так |
| `SubscriptionNotFound` на команді з Azure CLI | не зареєстрований провайдер ресурсу | `az provider register --namespace Microsoft.Storage` (або `Microsoft.Sql`, `Microsoft.Web`) |
| Завантаження файлу понад 6 МБ рве зʼєднання без відповіді | `RequestSizeLimit` відсікає запит до буферизації, відповідати вже нічим | фронт перевіряє розмір у себе; між 5 і 6 МБ приходить нормальна `400` |
| `A request body size limit could not be applied` у логах тестів | тестовий сервер не підтримує цю можливість | нормально, на проді ліміт діє |
