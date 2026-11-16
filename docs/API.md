# JobPortal API — контракт для фронтенду

Актуально на 16.11.2026. Документ описує те, що зараз працює на опублікованому API.

- **Базова адреса:** `https://jobportal-pl-api.azurewebsites.net`
- **Swagger:** https://jobportal-pl-api.azurewebsites.net/swagger/index.html
- **Формат:** JSON (`Content-Type: application/json`), крім завантаження резюме — там `multipart/form-data`

---

## 1. Авторизація

Після реєстрації або логіну приходить два токени:

```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "9xK2mP...",
  "accessTokenExpiresAt": "2026-11-16T18:15:00Z",
  "user": {
    "id": "3f2504e0-4f89-11d3-9a0c-0305e82c3301",
    "email": "ivan@example.com",
    "firstName": "Іван",
    "lastName": "Петренко",
    "role": 0
  }
}
```

**Access-токен** живе 15 хвилин і передається в кожному запиті:

```
Authorization: Bearer <accessToken>
```

**Refresh-токен** живе 7 днів. Коли access протермінувався — API віддає `401`. Тоді робите `POST /api/Auth/refresh` і отримуєте нову пару.

Три правила, важливі для фронту:

1. **Ротація.** Після `refresh` старий refresh-токен перестає діяти одразу. Зберігайте новий, інакше наступне оновлення дасть `401`.
2. **Захист від краденого токена.** Якщо надіслати вже використаний refresh-токен, сервер вважає його краденим і **відкликає всі сесії користувача** — його викине з усіх пристроїв. Тому не робіть два паралельних `refresh`: якщо кілька запитів одночасно отримали `401`, оновлюйте токен один раз і решту ставте в чергу.
3. **Прострочена сесія.** `401` означає тільно «токен недійсний». Після невдалого `refresh` ведіть користувача на вхід.

### Реєстрація кандидата

`POST /api/Auth/register/candidate` — без токена

```json
{
  "email": "ivan@example.com",
  "password": "MyPass123!",
  "firstName": "Іван",
  "lastName": "Петренко"
}
```

Відповідь: `200` + об'єкт з токенами (вище).

### Реєстрація роботодавця

`POST /api/Auth/register/employer` — без токена

```json
{
  "email": "hr@acme.com",
  "password": "MyPass123!",
  "firstName": "Олена",
  "lastName": "Коваль",
  "companyName": "Acme",
  "companyWebsite": "https://acme.com",
  "companyDescription": "Робимо софт"
}
```

Обов'язкові: `email`, `password`, `firstName`, `lastName`, `companyName`. Компанія створюється одразу разом з користувачем.

**Роль не передається з клієнта.** Її визначає ендпоінт: `register/candidate` завжди дає `JobSeeker`, `register/employer` завжди `Employer`. Отримати `Admin` через реєстрацію неможливо — такого поля немає ні в одному DTO.

### Інше

| Метод | Шлях | Тіло | Відповідь |
|---|---|---|---|
| POST | `/api/Auth/login` | `{ email, password }` | `200` + токени |
| POST | `/api/Auth/refresh` | `{ refreshToken }` | `200` + нові токени |
| POST | `/api/Auth/logout` | `{ refreshToken }` | `204` |
| POST | `/api/Auth/forgot-password` | `{ email }` | `204` завжди |
| POST | `/api/Auth/reset-password` | `{ token, newPassword }` | `204` |

`forgot-password` віддає `204` навіть якщо такої пошти немає — щоб через форму не можна було дізнатись, хто зареєстрований. Лист з посиланням приходить на пошту, токен у посиланні живе 30 хвилин.

`login` на невідому пошту і на невірний пароль дає однакову помилку — з тієї самої причини. Після 5 невдалих спроб акаунт блокується на 15 хвилин.

---

## 2. Значення переліків

У JSON усі переліки — **числа**.

| Перелік | Значення |
|---|---|
| `role` (UserRole) | `0` JobSeeker, `1` Employer, `2` Admin |
| `currency` | `0` PLN, `1` UAH, `2` USD, `3` EUR |
| `employmentType` | `0` FullTime, `1` PartTime, `2` Contract, `3` Internship |
| `workMode` | `0` Onsite, `1` Remote, `2` Hybrid |
| `status` (VacancyStatus) | `0` Draft, `1` Published, `2` Closed |
| `status` (ApplicationStatus) | `0` Pending, `1` Viewed, `2` Invited, `3` Rejected |
| `category` (VacancyCategory) | `0` IT, `1` Marketing, `2` Sales, `3` Finance, `4` Design, `5` Education, `6` Medicine, `7` Logistics, `8` Construction, `9` Service, `10` Other |
| `experienceLevel` | `0` NoExperience, `1` Junior, `2` Middle, `3` Senior, `4` Lead |
| `sortBy` (VacancySortBy) | `0` Date, `1` Salary |

---

## 3. Пагінація

Усі списки повертаються в одному форматі:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 137,
  "totalPages": 7,
  "hasNext": true
}
```

Параметри запиту: `page` (з 1) і `pageSize`. За замовчуванням 20, **максимум 50** — більше сервер усе одно обріже, тому нескінченний скрол будуйте на `hasNext`.

---

## 4. Помилки

Усі помилки приходять у форматі ProblemDetails:

```json
{
  "title": "ConflictException",
  "status": 409,
  "detail": "Ви вже відгукнулись на цю вакансію"
}
```

`detail` написаний українською і придатний для показу користувачу. Помилки валідації мають інший вигляд — з переліком полів:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Description": ["Довжина 'Description' має бути не меншою ніж 20 символів."]
  }
}
```

| Код | Коли |
|---|---|
| `400` | не пройшла валідація, поганий файл |
| `401` | немає токена, токен протермінований або недійсний |
| `403` | роль не підходить або це не ваш об'єкт |
| `404` | немає, або є але вам не видно (чернетка, закрита вакансія, чужий відгук) |
| `409` | конфлікт: повторний відгук, повторне обране, зайнята пошта |

**Чому `404`, а не `403`, на чужі чернетки й закриті вакансії:** щоб не можна було перебором id дізнатись, що такий об'єкт існує.

---

## 5. Кабінет (будь-яка роль)

### `GET /api/Account/me`

```json
{
  "id": "3f2504e0-4f89-11d3-9a0c-0305e82c3301",
  "email": "ivan@example.com",
  "firstName": "Іван",
  "lastName": "Петренко",
  "phoneNumber": "+48123456789",
  "role": 0,
  "emailConfirmed": false,
  "companyId": null,
  "createdAt": "2026-11-01T10:00:00Z"
}
```

`companyId` заповнений тільно в роботодавця.

### `PUT /api/Account/me`

```json
{ "firstName": "Іван", "lastName": "Петренко", "phoneNumber": "+48123456789" }
```

Пошту й роль через цей ендпоінт змінити неможливо.

---

## 6. Компанія

| Метод | Шлях | Хто | Опис |
|---|---|---|---|
| GET | `/api/Company/{id}` | будь-хто | публічна сторінка компанії |
| GET | `/api/Company/my` | Employer | своя компанія |
| PUT | `/api/Company/my` | Employer | змінити свою компанію |

```json
{
  "id": "8a1b...",
  "name": "Acme",
  "website": "https://acme.com",
  "description": "Робимо софт",
  "logoUrl": null,
  "createdAt": "2026-11-01T10:00:00Z"
}
```

`PUT /api/Company/my` приймає `{ name, website, description, logoUrl }`. Id у шляху немає навмисно: роботодавець може змінити тільно свою компанію, і вона визначається з токена. Чужу змінити неможливо в принципі — такого маршруту не існує.

---

## 7. Вакансії

| Метод | Шлях | Хто |
|---|---|---|
| GET | `/api/Vacancy/search` | будь-хто |
| GET | `/api/Vacancy/{id}` | будь-хто |
| GET | `/api/Vacancy/my` | Employer |
| POST | `/api/Vacancy` | Employer |
| PUT | `/api/Vacancy/{id}` | Employer (власник) |
| POST | `/api/Vacancy/{id}/publish` | Employer (власник) |
| POST | `/api/Vacancy/{id}/close` | Employer (власник) |

### Життєвий цикл

`Draft` → `publish` → `Published` → `close` → `Closed`

- у пошуку видно **лише** `Published`
- `GET /api/Vacancy/{id}` на чернетку або закриту дає `404` навіть за прямим посиланням
- у `GET /api/Vacancy/my` роботодавець бачить усі свої, включно з чернетками й закритими
- вакансії **не видаляються** — тільно закриваються

### Пошук

`GET /api/Vacancy/search`

| Параметр | Тип | Примітка |
|---|---|---|
| `search` | string | по заголовку |
| `city` | string | точний збіг |
| `employmentType` | число | |
| `workMode` | число | |
| `currency` | число | |
| `salaryFrom` | число | порівнюється з `salaryMax` вакансії |
| `category` | число | |
| `experienceLevel` | число | |
| `skills` | string, можна кілька | повторенням: `?skills=C%23&skills=SQL` |
| `sortBy` | число | `0` дата, `1` зарплата |
| `desc` | boolean | за замовчуванням `true` |
| `page`, `pageSize` | число | |

**Навички працюють як «І», не «АБО».** `?skills=C%23&skills=SQL` віддасть вакансії, де є **обидві**.

`salaryFrom` порівнюється з верхньою межею вілки: вакансія 4000–8000 підходить під запит «від 6000».

При сортуванні за зарплатою вакансії без зарплати завжди в кінці, в обидва боки.

### Вакансія у відповіді

```json
{
  "id": "c5aef33c-508d-445c-8cbf-f5844c6abea7",
  "title": "Junior .NET Developer",
  "description": "Шукаємо джуна...",
  "city": "Lublin",
  "salaryMin": 5000,
  "salaryMax": 9000,
  "currency": 0,
  "employmentType": 0,
  "workMode": 1,
  "status": 1,
  "category": 0,
  "experienceLevel": 1,
  "skills": ["C#", "SQL"],
  "companyId": "8a1b...",
  "companyName": "Acme",
  "companyLogoUrl": null,
  "createdAt": "2026-11-10T09:00:00Z",
  "publishedAt": "2026-11-10T09:05:00Z"
}
```

### Створення і зміна

`POST /api/Vacancy` і `PUT /api/Vacancy/{id}` приймають однакове тіло:

```json
{
  "title": "Junior .NET Developer",
  "description": "Шукаємо джуна, опис не коротше 20 символів",
  "city": "Lublin",
  "salaryMin": 5000,
  "salaryMax": 9000,
  "currency": 0,
  "employmentType": 0,
  "workMode": 1,
  "category": 0,
  "experienceLevel": 1,
  "skills": ["C#", "SQL"]
}
```

Обмеження валідації:

- `title` — 3…150 символів
- `description` — **20**…5000 символів
- `city` — до 100
- `salaryMax` не менше за `salaryMin`
- `skills` — до 20 штук, кожна до 50 символів, **порожній рядок у списку дає `400`**
- `skills: null` дає `400`. Якщо навичок немає — присилайте `[]` або не присилайте поле взагалі
- нові вакансії створюються в стані `Draft`, їх треба окремо опублікувати

`PUT` замінює навички **цілком**: що не прислали — те видалено.

---

## 8. Обране (JobSeeker)

| Метод | Шлях | Відповідь |
|---|---|---|
| POST | `/api/Favorite/{vacancyId}` | `204` |
| DELETE | `/api/Favorite/{vacancyId}` | `204` |
| GET | `/api/Favorite/my` | список `VacancyDto` з пагінацією |

- повторне додавання дає `409` — кнопку-серце робіть так, щоб вона не стріляла двічі, або просто ігноруйте `409`
- додати чернетку або закриту вакансію — `404`
- видалити те, чого немає в обраному — `404`
- `GET /api/Favorite/my` повертає повні вакансії, відсортовані за датою додавання в обране

---

## 9. Профіль кандидата і резюме (JobSeeker)

### `GET /api/Profile/my`

```json
{
  "headline": "Junior .NET Developer",
  "about": "Вчусь, пишу бекенд",
  "hasCv": true,
  "cvFileName": "rezume.pdf",
  "cvSizeBytes": 248310,
  "cvUploadedAt": "2026-11-12T14:30:00Z"
}
```

Якщо профіль ще не заповнювали — `200` з усіма `null` і `hasCv: false`. Це не помилка, окремого «створити профіль» немає.

### `PUT /api/Profile/my`

```json
{ "headline": "Junior .NET Developer", "about": "Вчусь, пишу бекенд" }
```

Обидва поля необов'язкові, ліміти 200 і 2000 символів.

### `POST /api/Profile/my/cv` — завантажити або замінити

`multipart/form-data`, поле називається **`file`**.

```js
const fd = new FormData();
fd.append('file', pickedFile);          // поле саме "file"

await fetch(`${API}/api/Profile/my/cv`, {
  method: 'POST',
  headers: { Authorization: `Bearer ${accessToken}` },  // Content-Type НЕ ставити,
  body: fd                                              // браузер додасть сам з boundary
});
```

Відповідь `200` — той самий об'єкт профілю.

Перевірки й помилки:

| Ситуація | Код | `detail` |
|---|---|---|
| не PDF за розширенням або типом | `400` | `Дозволено тільки PDF` |
| PDF лише за назвою (перевіряємо байти файлу) | `400` | `Файл не схожий на PDF` |
| більше 5 МБ | `400` | `Файл більший за 5 МБ` |
| порожній файл | `400` | `Файл порожній` |

**Перевіряйте розмір у себе перед відправкою.** Файл понад 6 МБ сервер обриває на рівні з'єднання — у браузері це `Failed to fetch` без тіла відповіді, текст помилки зловити не вийде. Між 5 і 6 МБ приходить нормальна `400`.

Невдале завантаження **не змінює** наявне резюме: якщо щось уже було, воно лишається на місці.

### `GET /api/Profile/my/cv` — скачати своє

Віддає файл. Ім'я у заголовку `Content-Disposition`. Якщо резюме немає — `404`.

### `DELETE /api/Profile/my/cv`

`204`. Текстові поля профілю при цьому лишаються.

---

## 10. Відгуки

| Метод | Шлях | Хто |
|---|---|---|
| POST | `/api/Application` | JobSeeker |
| GET | `/api/Application/my` | JobSeeker |
| GET | `/api/Application/vacancy/{vacancyId}` | Employer (власник вакансії) |
| PUT | `/api/Application/{id}/status` | Employer (власник вакансії) |
| GET | `/api/Application/{id}/cv` | Employer (власник вакансії) |

### Відгукнутись

`POST /api/Application`

```json
{ "vacancyId": "c5aef33c-508d-445c-8cbf-f5844c6abea7", "coverLetter": "Доброго дня..." }
```

**Резюме обов'язкове.** Без завантаженого CV приходить `400` з `Спершу завантажте резюме у профіль`.

Не ловіть цю помилку текстом — перед показом кнопки «Відгукнутись» перевіряйте `hasCv` з `GET /api/Profile/my`. Якщо `false`, відкривайте діалог завантаження резюме, а `400` лишіть як страховку на випадок, коли резюме видалили в іншій вкладці.

| Ситуація | Код |
|---|---|
| успіх | `200` + `ApplicationDto` |
| немає резюме | `400` |
| вже відгукувались | `409` |
| вакансія чернетка або закрита | `404` |

### Відгук у відповіді

```json
{
  "id": "6de7330a-edbf-493b-b9ca-f08d7d0b3045",
  "status": 0,
  "coverLetter": "Доброго дня...",
  "createdAt": "2026-11-13T12:00:00Z",
  "vacancyId": "c5aef33c-...",
  "vacancyTitle": "Junior .NET Developer",
  "companyName": "Acme",
  "candidateId": "3f2504e0-...",
  "candidateFirstName": "Іван",
  "candidateLastName": "Петренко",
  "candidateEmail": "ivan@example.com",
  "candidatePhone": "+48123456789",
  "hasCv": true,
  "cvFileName": "rezume.pdf"
}
```

### Зміна статусу

`PUT /api/Application/{id}/status` з тілом `{ "status": 2 }`. Міняти може тільно власник вакансії, інакше `403`.

### Скачування резюме роботодавцем

`GET /api/Application/{id}/cv`

Віддає PDF. Важливо, як це працює:

- доступно **тільно тому роботодавцю**, на чию вакансію подались; будь-який інший отримає `403`
- кандидат на цьому ендпоінті теж отримає `403` — він качає своє через `/api/Profile/my/cv`
- віддається **копія резюме на момент відгуку**. Якщо кандидат потім замінив або видалив своє CV, роботодавець усе одно бачить той документ, який йому надіслали, і з тим самим ім'ям файлу
- показуйте кнопку скачування по полю `hasCv` у `ApplicationDto`

---

## 11. Повідомлення в реальному часі (SignalR)

Хаб: `/hubs/applications`. Токен передається в query string, бо WebSocket не дозволяє власні заголовки:

```js
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API}/hubs/applications`, { accessTokenFactory: () => accessToken })
  .withAutomaticReconnect()
  .build();

// роботодавцю — новий відгук на його вакансію
connection.on('NewApplication', application => { /* ApplicationDto */ });

// кандидату — зміна статусу його відгуку
connection.on('StatusChanged', application => { /* ApplicationDto */ });

await connection.start();
```

Повідомлення — приємний бонус, а не джерело істини: вони не зберігаються і втрачаються при перезапуску сервера. Списки все одно перечитуйте запитами.

---

## 12. CORS

Дозволені origin зараз:

```
https://project-clone-work-ua.vercel.app
http://localhost:5173
```

Origin — це схема, хост і порт, **без слеша в кінці** і без шляху. `https://project-clone-work-ua.vercel.app/` зі слешем не зматчиться.

`AllowCredentials` увімкнено, тому запити з `credentials: 'include'` працюють.

**Прев'ю-деплої Vercel не входять у список.** Адреси типу `project-clone-work-ua-git-main-xxx.vercel.app` — це інший origin, і з них запити заблокує браузер. Якщо треба тестувати з прев'ю — скажіть, додам адресу.

Щоб додати новий origin, бекендер виконує:

```powershell
az webapp config appsettings set -g jobportal-rg -n jobportal-pl-api --settings "Cors__Origins__2=https://нова-адреса"
```

---

## 13. Короткий чекліст на інтеграцію

1. Реєстрація/логін → зберегти `accessToken` і `refreshToken`
2. `Authorization: Bearer` у кожен запит
3. На `401` — один `refresh`, решту запитів у чергу; якщо `refresh` теж `401` — на сторінку входу
4. Роль брати з `user.role`: `0` кандидат, `1` роботодавець
5. Кабінет кандидата: `GET /api/Profile/my`, `GET /api/Application/my`, `GET /api/Favorite/my`
6. Кабінет роботодавця: `GET /api/Company/my`, `GET /api/Vacancy/my`, `GET /api/Application/vacancy/{id}`
7. Кнопку «Відгукнутись» вмикати по `hasCv`
8. Пагінацію будувати на `hasNext`, не на `totalCount`
9. `pageSize` більше 50 не має сенсу
10. Усі переліки — числа, не рядки
