# Auth

## Задача 1. Запуск

Требуется научиться запускать приложение и убедиться, что все хорошо.

Приложение использует https, поэтому для корректной работы понадобится сертификат.
.NET Core умеет создавать сертификаты для localhost. Только надо установить такой сертификат в доверенные.
Для этого запусти команду:
```bash
dotnet dev-certs https --trust
```

Запусти приложение под отладкой. Должен запусться браузер и открыть стартовую страницу приложения.
Убедись, что при запуске в папке `PhotoApp` автоматически создался файл `PhotoApp.db` с базой данных Sqlite.

Открой файл `PhotoApp.db` с помощью сервиса https://sqliteonline.com/.
Убедись, что в нем есть таблица `Photos`, выведи записи из нее.


### Задача 2.1. Scaffolding

Требуется сгенерировать код Identity, а затем ее корректно подключить к приложению.


Прежде всего потребуется установить новый инструмент для .NET Core CLI — генератор кода:
```bash
dotnet tool install -g dotnet-aspnet-codegenerator
```

Кроме того, в проект надо добавить NuGet-пакет для кодогенерации.
Выполни в папке с проектом:
```bash
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
```

Наконец, можно выполнить команду генерации кода Identity:
```bash
dotnet aspnet-codegenerator identity -dc UsersDbContext -u PhotoAppUser -sqlite
```
Дополнительные параметры команды указывают:
- имя DbContext, который будет использоваться для хранения информации о пользователях,
- класс пользователя, хранимого в базе данных,
- что, в качестве базы данных надо использовать Sqlite, а не SQL Server.


В проекте `PhotoApp` в папке `Areas/Identity` был сгенерирован требующийся код.

Посмотри структуру папки  `Areas/Identity`.
Обрати внимание на файл `Areas/Identity/IdentityHostingStartup.cs`.
Код из него будет автоматически запускаться после `Startup.cs` и завершать конфигурирование.
Также обрати внимание, что страницы сгенерированы по технологии `Razor Pages`.
В отличие от MVC, где «верстка» (View) и «обработка» (Controller) находится в разных местах,
здесь все находится в двух соседних файлах. Например, для страницы `Identity/Pages/Account/Login.cshtml`,
верстка находится в `Login.cshtml`, а обработка в `Login.cshtml.cs`.


Также в `Views/Shared` был сгенерирован `_LoginPartial.cshtml`, содержищий верстку для отображения
ссылок для регистрации/входа/выхода пользователя в меню приложения.

Ссылки регистрации/входа/выхода надо добавить в меню приложения.
Для этого открой `Views/Shared/_Layout.cshtml` и добавь вставку `_LoginPartial.cshtml` с помощью tag helper
сразу после других ссылок:
```cshtml
<ul class="navbar-nav mr-auto">
    ...
</ul>
<partial name="_LoginPartial"/>
```


Можешь убедиться, что ссылки появились и при переходе по ним открываются страницы Indentity.


Только ничего не работает, потому что пользователей нет, да и таблица базы данных для них еще не создана.


В коде появился новый контекст. Чтобы обновить БД для хранения данных из него, надо создать миграцию:
```bash
dotnet ef migrations add Users --context UsersDbContext
```
Миграция — это план обновления. Его можно применять к БД, которыми будет пользоваться приложение.

Чтобы миграция успешно создалась:
1. Приложение должно компилироваться без ошибок
2. При старте приложения не должно быть ошибок,
   т.е. код конфигурирования (в `Startup.cs` и `IdentityHostingStartup.cs`) должен работать корректно
3. Приложение не должно быть запущенным
Все это нужно, чтобы команда миграции смогла построить и запустить проект,
а затем получить через рефлексию всю необходимую информацию о контексте.


После создания миграции ее надо запустить на имеющейся базе данных Sqlite.
Сделай это с помощью следующей команды:
```bash
dotnet ef database update --context UsersDbContext
```
Теперь база данных обновлена и в ней можно хранить информацию о пользователях.
Посмотри с помощью https://sqliteonline.com/ какие таблицы были созданы. Заметь, что их достаточно много.


Для целей разработки можно вместо `dotnet ef database update` использовать такой вызов метода:
```cs
dbContext.Database.Migrate()
```
Здесь `dbContext` — экземпляр класса-наследника `DbContext`.

Посмотри как этот метод используется в файле `Data/DataExtensions.cs` для `PhotosDbContext`,
и напиши аналогичный код для `UsersDbContext`.
В результате, если файл `PhotoApp.db` удален, то при запуске приложения
он автоматически восстановится со всеми таблицами.


Теперь надо добавить тестовых пользователей.
Это умеет делат метод `SeedWithSampleUsersAsync` из `Data/DataExtensions.cs`.
Вызови его в `PrepareDB`, чтобы при старте приложения создавались тестовые пользователи.
Подсказка: `UserManager<PhotoAppUser>` можно достать из `ServiceProvider`.


Чтобы под пользователем можно было зайти, подключи middleware аутентификации в `Startup.cs`.
Его вызов обязательно должен быть после подключением middleware для отдачи статических файлов (`UseStaticFiles`),
но перед подключением middleware для MVC (`UseMvc`).
Нужный вызов:
```cs
app.UseAuthentication();
```


Теперь попробуй зайти под пользователем `vicky` или `cristina`. Пароли можно найти в `Data/DataExtensions.cs`.


Но даже если зайти под нужным пользователем, его фотки не будут показываться, пока не поправить `PhotoController`.
Измени метод `GetOwnerId` так, чтобы он возвращал идентификатор залогиненного пользователя:
```cs
private string GetOwnerId()
{
    return User.FindFirstValue(ClaimTypes.NameIdentifier);
}
```


Снова зайди под `vicky`, а затем под `cristina`. Фотографии должны быть разными.


Осталось пара нюансов.

1. При заходе на страницы управления аккаунтом (кликни на имя пользователя в меню приложения, чтобы туда попасть)
показывается неправильная шапка страницы.
Чтобы починить в файле `/Areas/Identity/Pages/Account/Manage/_Layout.cshtml` поменяй `Layout`.
Корректное значение для приложения — `"/Views/Shared/_Layout.cshtml"`

2. Logout работает некорректно. После него не происходит перехода на главную страницу приложения,
а в верхнем меню остается имя пользователя. Это происходит потому, что в `_LoginPartial.cshtml`
указан некорректный `asp-route-returnUrl`. Должен быть `@Url.Action("Index", "Photos", new { area = "" })`.


### Задача 2.2. Авторизация

Теперь надо сделать, чтобы анонимный пользователь автоматически пересылался на страницу входа
при выполнении любых действий с фотографиями. Доступной должна остаться только главная страница.

Для этого достаточно пометить атрибутом `[Authorize]` все методы или контроллеры, которые требуется защитить.
Если пометить атрибутом `[Authorize]` контроллер, но надо разрешить некоторый метод, то метод помечается
атрибутом `[AllowAnonymous]`.

Защити все действия над фотографиями из `PhotoController`, кроме `Index`.


### Задача 3.1. Пароли

Настройки по умолчанию для паролей хороши:
- есть требования на длину и используемые символы,
- пароли не хранятся о открытом виде, а хэшируются с солью.
Но они не всегда подходят.

В большинстве случаев достаточно конфигурирования. Начни с этого.

Настройки по умолчанию для паролей можно посмотреть тут:
https://docs.microsoft.com/ru-ru/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-2.2#password

Заодно в том же документе можно посмотреть настройки по умолчанию для входа:
https://docs.microsoft.com/ru-ru/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-2.2#sign-in

Чтобы облегчить себе жизнь во время прохождения блока:
1. Скопируй явную конфигурацию из документации в `IdentityHostingStartup.cs`
2. Выстави настройки для паролей `RequireDigit`, `RequireNonAlphanumeric`,
   и `RequireUppercase` в `false`. Оставь `RequireLowercase` в `true`!
3. Заодно выстави в настройках входа `RequireConfirmedEmail` в `false`,
   чтобы при регистрации новых пользователей не требовалось подтверждать email.
   В реальных проектах так делать не надо, это только для разработки и обучения :)

При желании можешь поменять пароли для `vicky`, `cristina` и `dev` в файле `DataExtensions.cs`, чтобы было проще.
Зарегистрируй нового пользователя с просстым паролем из 6 символов: у тебя должно получиться.
Затем выйди из него и зайди снова. Вход должен получиться, несмотря на то, что ты не подтверждал email.


Все ошибки, которые ты видел, были на английском языке. Это не очень удобно для русскоговорящих пользователей.

В большинстве случаев тексты ошибок на английском прописаны в файлах `Identity/Pages`.
Например, в файле `Register.cshtml.cs` в классе `InputModel` с помощью атрибутов.
У любого атрибута для валидации есть свойство `ErrorMessage`, в котором можно прописать текст сообщения об ошибке
на русском языке. Таким образом эти тексты ошибок легко локализуются.
Задай текст сообщения для атрибута `Required` в свойстве `Email` класса `InputModel`.

Но кроме атрибутов для локализации нужно поменять реализацию `IdentityErrorDescriber`.
Уже есть готовая реализация, позаимствованная со StackOverflow: `Services/RussianIdentityErrorDescriber.cs`.
В файле `IdentityHostingStartup.cs` в конфигурировании Identity (найди `services.AddDefaultIdentity<PhotoAppUser>()`)
добавь строчку `.AddErrorDescriber<RussianIdentityErrorDescriber>()`.

Теперь попробуй зарегистрировать нового пользователя:
- Сначала заполни email, а затем сделай пустым. Ты должен увидеть сообщение об ошибке из атрибута `Required`.
  Благодаря `jquery.validate` сообщение появляется до отправки формы.
- Теперь введи корректный email, но в качестве пароля используй 6 цифр, например, 123456.
  Отправь форму. Если все правильно, то в ответ получишь сообщение
  из `RussianIdentityErrorDescriber`: «Пароль должен содержать хотя бы один символ в нижнем регистре»

Как локализовать весь остальной пользовательский интерфейс ясно: надо локализовывать файлы из папки `Identity/Pages`.
Сейчас, по понятным причинам, этого делать не нужно.


Все же может потребоваться добавить новые правила проверки паролей.
Например, проверить, что новый пароль не совпадает с логином пользователя.
Проверка уже реализована в `Services/UsernameAsPasswordValidator.cs`. Изучи ее код.
А затем добавь строчку `.AddPasswordValidator<UsernameAsPasswordValidator<PhotoAppUser>>()`
в конфигурировании Idenity и убедись, что нельзя зарегистрировать пользователя, если пароль совпадает с email.


Может понадобится изменить алгоритм хэширования паролей. Например, если есть база пользователей,
которым надо дать доступ к приложению, но при их регистрации использовался свой собственный алгоритм хэширования.

Если использовалась предыдущя версия Identity, то алгоритм хэширования можно просто донастроить,
указав верную версию и количество итераций при хэшировании:
```cs
services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    options.IterationCount = 12000;
});
```

Полностью заменить алгоритм хэширования на свой можно так:
```cs
services.AddScoped<IPasswordHasher<PhotoAppUser>, SimplePasswordHasher<PhotoAppUser>>();
```

Вот только `SimplePasswordHasher` из папки `Services` не до конца реализован.
Имея реализацию метода `HashPassword`, дореализуй метод `VerifyHashedPassword`.

Затем подключи `SimplePasswordHasher`, с помощью Debug убедись, что теперь исползуется он,
а вход под пользователями с новым алгоритмом хэширования работает.


### Задача 3.2. Сессии

После успешной аутентификации информация о пользователе по умолчанию хранится в cookie.
Браузер постоянно передает эту cookie на сервер и за счет этого все действия пользователя можно авторизовать.


По умолчанию все работает некоторым образом. Настройки по умолчанию можно посмотреть тут:
https://docs.microsoft.com/ru-ru/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-2.2#cookie-settings

Давай донастроим. Для этого:
1. Скопируй явную конфигурацию из документации в `IdentityHostingStartup.cs`.
2. Выстави `options.Cookie.Name` значение `"PhotoApp.Auth"`, чтобы сессия хранилась в cookie и известным именем.
3. Обрати внимание на настройку `options.Cookie.HttpOnly = true`. Это значит, что cookie не будет доступна клиентским скриптам,
   что обычно правильно и защищает пользователя от атак со скриптов.
4. Настройка `options.SlidingExpiration = true` означает, что сессия не протухнет,
   пока пользователь активно использует приложение. Это тоже хорошее поведение.

Теперь залогинься под любым пользователем и найди в меню приложения ссылку на страницу Decode и перейди по ней.
На этой странице аутентификационная кука расшифровывается, а затем информация из нее выводится.
Ниже выводится информация о пользователе из поля User, т.е. как ее видят контроллеры.
Сейчас видно, что сейчас в cookie и в User хранится одна и та же информация.


Если надо хранить в сессии много данных о пользователе, то аутентификационная кука станет достаточно большой.
Неэкономично заставлять браузер передавать все эти данные с каждым запросом в виде cookie.
В этом случае можно хранить данные о сессии на сервере.

Для хранения сессии на сервере хорошо подойдет распределенной InMemory хранилище.
InMemory — для скорости, распределенное — для отказоустойчивости. Например, подойдет Redis.
Но для учебных целей воспользуемся все тем же Sqlite.

Готовые хранилища уже реализованы в `Services/EntityTicketStore.cs` и `Services/MemoryCacheTicketStore.cs`
Посмотри как они устроены.

`MemoryCacheTicketStore` проще, потому что хранит всю информацию о сессиях в оперативной памяти.
Очень быстро, но перезагрузка веб-сервера заставит пользователей входить заново. Не надо так.

Поэтому подключать стоит `EntityTicketStore`, сделай это:
```cs
services.AddTransient<EntityTicketStore>();
services.ConfigureApplicationCookie(options =>
{
    var serviceProvider = services.BuildServiceProvider();
    options.SessionStore = serviceProvider.GetRequiredService<EntityTicketStore>();
    ...
});
```

Так как это хранилище использует Entity Framework, надо его сконфигурировать,
а затем выполнить миграцию и обновление базы данных:
1. Сконфигурируй `TicketsDbContext` в `IdentityHostingStartup.cs` аналогично `UsersDbContext`,
   добавь значение для `TicketsDbContextConnection` в `appsettings.json`
2. `dotnet ef migrations add Tickets --context TicketsDbContext`
3. `dotnet ef database update --context TicketsDbContext`,
  либо добавить `dbContext.Database.Migrate()` в `Data/DataExtensions.cs`

После подключения снова залогинься и перейди на страницу Decode.
Обрати внимание, что теперь в аутентификационной куке хранится только идентификатор сессии.
Вся остальная информация о пользователе хранится и незаметно достается из Sqlite.


### Задача 4.1. Роли

Не весь функционал должен быть доступен каждому пользователю.
Требуется ограничить права различных групп пользователей.


Один из способов — ввести систему ролей.
Добавь новую роль `Dev`, присвой ее пользователю `dev@gmail.com` и сделай так,
чтобы только пользователи с ролью `Dev` имели доступ к `DevController`.

Подсказки:
- В конфигурировании Identity в `IdentityHostingStartup.cs` надо добавить `.AddRoles<IdentityRole>()`
  сразу после `.AddDefaultIdentity<PhotoAppUser>()`
- Роль нужно предварительно создать в БД. Сделай так, чтобы метод `SeedWithSampleRolesAsync`
  из `DataExtensions.cs` выполнялся при старте приложения, причем до `SeedWithSampleUsersAsync`
- Добавить пользователю новую роль можно командой `await userManager.AddToRoleAsync(user, "RoleName")`
- Защитить метод или контроллер можно с помощью атрибута с параметром: `[Authorize(Roles = "RoleName")]`

Когда закончишь, убедись, что только пользователь `dev@gmail.com` может пользоваться страницей Decode,
а у других пользователей возникает сообщение об ошибке.


Будет хорошо, если пользовали, которым недоступен Decode вообще не будут видеть ссылку на страницу. Сделай так!
Проверить во view, что у текущего пользователя есть роль можно так: `User.IsInRole("RoleName")`


### Задача 4.2. Политики

Более гибко настраивать права пользователей позволяют политики на основании различных claims (утверждениях) пользователя.

Сейчас любому пользователю при заходе на страницу отдельной фотографии доступно изменение подписи к фотографии.
Сделай так, чтобы возможность редактировать подписи к фотографиям была доступна только beta-тестерам.

Для начала в `IdentityHostingStartup.cs` нужно зарегистрировать некоторую политику:
```cs
services.AddAuthorization(authorizationOptions =>
{
    authorizationOptions.AddPolicy(
        "Beta",
        policyBuilder =>
        {
            policyBuilder.RequireAuthenticatedUser();
            policyBuilder.RequireClaim("testing", "beta");
        });
});
```

Эта политика требует, чтобы у пользователь был аутентифицирован и у него был claim `testing` со значением `beta`.
Сейчас таких пользователей нет.

Сделай так, чтобы при старте приложения пользователю `vicky` добавлялся такой claim.
Подсказка: `await userManager.AddClaimAsync(user, new Claim("claimType", "claimValue"))`
Claim, добавленные таким образом хранятся в отдельной таблице. Можешь в этом убедиться.

Теперь защити действия `EditPhoto` в `PhotoController` с помощью атрибута `[Authorize(Policy = "Beta")]`.


Когда закончишь, убедись, что только пользователь `vicky@gmail.com` может редактировать подписи к фотографиям,
а у других пользователей возникает сообщение об ошибке.


Будет хорошо, если пользовали, которым недоступно редактирование подписей вообще не видели ссылки на это действие.
Проверить во view выполнение политики для пользователя можно так:
```cshtml
(await AuthorizationService.AuthorizeAsync(User, "PolicyName")).Succeeded
```
Только надо добавить в начале view подключение зависимостей:
```cshtml
@using Microsoft.AspNetCore.Authorization
@inject IAuthorizationService AuthorizationService
```
Скрой действие «Изменить подпись» на странице отдельной фотографии.


Когда закончишь с этим добавь еще одну политику: пусть только платым пользователям будет доступна загрузка фотографий.
Назови политику `CanAddPhoto`, в качестве типа claim использй `subscription`, в качестве значения `paid`.
Аналогично предыдущей политике, защити методы контроллера для загрузки фотографий.
и скрой ссылку «Добавить фото» в меню приложения.

А вот claim в пользователя надо выставить иначе. Путь он не хранится отдельно в таблице, а вычисляется по свойствам из `PhotoAppUser`.

Для этого:
1. Добавь в класс `PhotoAppUser` булево поле `Paid`.
2. Создай миграцию, т.к. надо добавить новую колонку в таблицу пользователей:
   `dotnet ef migrations add Paid --context UsersDbContext`
2. Разбери generic-параметр `TUser` в методе `SeedWithSampleUsersAsync`, заменив его использования
   на тип `PhotoAppUser`.
3. Сделай так, чтобы пользователю `cristina` при создании в поле `Paid` выставлялось значение `true`. 
4. Самое важное! Допиши `CustomClaimsPrincipalFactory` в `Services/Authorization/CustomClaimsPrincipalFactory.cs`.
   Сначала замени `IdentityUser` на `PhotoAppUser`, а затем сделай так, чтобы пользователю с `Paid == true`
   выставлялся claim `subscription` со значением `paid`.
5. Зарегистрируй фабрику в `IdentityHostingStartup.cs`:
   `services.AddScoped<IUserClaimsPrincipalFactory<PhotoAppUser>, CustomClaimsPrincipalFactory>()`

Убедись, что пользователю `cristina` доступно добавление фото, `vicky` не доступно.

Как видишь, определив собственную `UserClaimsPrincipalFactory`,
можно выставлять claims для пользователя по произвольным правилам.


### Задача 4.3. Обработчик для требования

В приложении до сих пор любой аутентифицировавшийся пользователь может открыть любую фотографию,
если у него будет прямая ссылка до нее.

Убедись в этом:
1. Зайди под пользователем `vicky`
2. Перейди на страницу с одной фотографией и сохрани URL страницы
3. Выполни logout и зайди под пользователем `cristina`
4. Используя сохраненный URL, чтобы открыть фотографию. Она доступна другому пользователю!

Чтобы создать политику, которая бы запрещала доступ к фото другим пользователям,
потребуется `AuthorizationHandler`.


Для начала создай новую политику `MustOwnPhoto`, а в ней потребуй два условия:
```cs
policyBuilder.RequireAuthenticatedUser();
policyBuilder.AddRequirements(new MustOwnPhotoRequirement());
```

`MustOwnPhotoRequirement` — некоторое требование, которое будет проверяться динамически с помощью обработчика.
Обработчиком требования является `MustOwnPhotoHandler`, потому что он наследуется
от класса `AuthorizationHandler<MustOwnPhotoRequirement>`.

Но, чтобы обработчик создавался его надо зарегистрировать в качестве `IAuthorizationHandler`:
```cs
services.AddScoped<IAuthorizationHandler, MustOwnPhotoHandler>();
```

Защити действия `GetPhoto`, `EditPhoto`, `DeletePhoto` в `PhotoController` с помощью новой политики.
Заметь, что это нормально использовать несколько атрибутов `Authorize` у метода.
В этом случае для выполнения действия должны быть выполнены требования каждого атрибута.

Допиши `MustOwnPhotoHandler` так, чтобы требование выполнялось,
если текущий пользователь является владельцем фотографии.


### Задача 5. Аутентификация через Google

ASP.NET Core включает встроенную поддержку для OAuth, за счет чего к нему
легко подключить внешних провайдеров аутентификации.
А для некоторых, включай Google и Facebook есть даже готовые методы,
позволяющие подключить провайдера, написав пару строчек.

Правда в связи с закрытием Google+ строчек в версии 2.2 немного больше...

Добавь следующий код в `IdentityHostingStartup.cs`:
```cs
services.AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.SignInScheme = IdentityConstants.ExternalScheme;

        options.ClientId = context.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];

        options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

        options.ClaimActions.Clear();
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
        options.ClaimActions.MapJsonKey("urn:google:profile", "link");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    });
```
Это почти все, что нужно, чтобы заработала аутентификация через Google в случае Identity,
потому что отображение нужных кнопок для внешних провайдеров аутентификации уже реализовано.


Осталось только зарегистрировать приложение в Google, получить Client ID и Client Secret,
а затем положить их в настройки, чтобы следующие строчки работали корректно:
```cs
options.ClientId = configuration["Authentication:Google:ClientId"];
options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
```

Для этого:
1. Перейди на страницу https://developers.google.com/identity/sign-in/web/sign-in#before_you_begin
2. Нажми кнопку «Configure Project»
3. Введи имя нового проекта
4. Выбери опцию Web server и введи https://localhost:5001/signin-google в качестве «Authorized redirect URIs»
5. Нажми на кнопку «Create», а затем получи Client ID и Client Secret.


https://localhost:5001/signin-google — это путь, по которому Google отправит данные пользователя
после успешной аутентификации. Такой адрес используется по умолчанию в ASP.NET Core, соответственно,
данные от Google будут успешно получены и обработаны Authentication Middleware.


Client ID и Client Secret используются авторизации приложения в Google.
Их можно сохранить в `appsettings.json`, по путям `Authentication:Google:ClientId`
и `Authentication:Google:ClientSecret` и все будет работать.
Но файлы, хранящиеся в репозитории, в том числе `appsettings.json` — это плохое место для хранения паролей и секретов.

Поэтому лучше воспользоваться специальным хранилищем для секретов вот так:
```bash
dotnet user-secrets set "Authentication:Google:ClientId" "<client id>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<client secret>"
```

В этом случае значения будут сохранены в папке тут:
- `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json` в Windows
- `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json` в Linux, Mac

В Visual Studio секретами можно управлять, если кликнуть правой кнопкой мыши по проекту в «Solution Explorer»
и выбрать пункт «Manage Secrets».


Далее своими проектами в Google можно будет управлять через специальный «пульт»:
https://console.developers.google.com/apis/credentials


После верного задания Client ID и Client Secret аутентификация через Google
должна появиться на странице логина и корректно работать.


### Задача 6. Письма

Хорошая практика — предлагать пользователю подтвердить адрес своей электронной почты,
чтобы случайная опечатка при вводе email или забытый пароль не приводили к потере доступа к аккаунту.

Identity пытается отправлять письма с кодом подтверждения всем новым пользователям с помощью `IEmailSender`.
По умолчанию он реализован так, что ничего не отправляет.


В `Services/SimpleEmailSender` есть реализация, которая умеет отправлять письма через внешний SMTP-сервер.
Подключи ее:
```cs
services.AddTransient<IEmailSender, SimpleEmailSender>(serviceProvider =>
    new SimpleEmailSender(
        serviceProvider.GetRequiredService<ILogger<SimpleEmailSender>>(),
        serviceProvider.GetRequiredService<IHostingEnvironment>(),
        context.Configuration["SimpleEmailSender:Host"],
        context.Configuration.GetValue<int>("SimpleEmailSender:Port"),
        context.Configuration.GetValue<bool>("SimpleEmailSender:EnableSSL"),
        context.Configuration["SimpleEmailSender:UserName"],
        context.Configuration["SimpleEmailSender:Password"]
    ));
```
Большинство настроек для подключения к SMTP-серверу Google уже прописаны в `appsettings.json`.
Пропиши в файл или в User Secrets адрес своей электронной почты Google в `UserName`
и соответствуйщий пароль в `Password`.

Также, чтобы «стороннее приложение», которое ты пишешь, смогло отправлять письма придется
понизить уровень безопасности аккаунта на странице https://myaccount.google.com/lesssecureapps

Зарегистрируй нового пользователя с существующим email и убедись, что на него пришло письмо
для подтверждения адреса электронной почты.


Если нужно, чтобы без подтверждения email нельзя было войти в аккаунт,
следует изменить настройку `RequireConfirmedEmail`.


### Задача 7. Json Web Token

Сейчас тебе предстоит добавить нестандартный способ аутентификации в сервисе.
Работать он должен так: пользователь переходит по секретному URL, где ему выставляется cookie с JWT-токеном.
Этот короткоживущий токен дает доступ разработчика к сервису на полминуты.


Найди и открой `HackController`. В метода `GenerateToken` с суперсекретным адресом вызывается генерация JWT-токена.
Затем этот токен добавляется в cookie.

Для начала надо доработать генерацию токена в методе `TemporaryTokens.GenerateEncoded`.
1. Сделай так, чтобы токен не действовал до текущего момента.
   Для этого надо передать текущее время в UTC в `notBefore`.
2. Сделай так, чтобы токен действовал всего лишь 30 секунд, задав правильно `expires`.
3. Заполни `claims`:
    - Утверждению `ClaimTypes.NameIdentifier` (идентификатор пользователя) задай значение `Guid.NewGuid().ToString()`
    - Утверждению `ClaimsIdentity.DefaultNameClaimType` (имя пользователя) задай какое-нибудь значение
    - Утверждению `ClaimsIdentity.DefaultRoleClaimType` (роль пользователя) задай значение `"Dev"`
4. Чтобы токен нельзя было просто подделать можно добавить зашифрованный с помощью симметричного ключа отпечаток.
   Воспользуйся алгоритмом связкой HMAC SHA-256.
   SHA-256 — хэш-функция, HMAC — алгоритм, использующий симметричный ключ и хэш-функцию для получения отпечатка.
   Все уже реализовано, надо только правильно задать `signingCredentials`.
   Используй ключ из свойства `SigningKey`, а имя алгоритма есть в константе `SecurityAlgorithms.HmacSha256`.

Теперь, если обратиться по пути `/hack/super_secret_qwe123` будет возвращен токен. Он также окажется в cookie.

Расшифруй этот токен с помощью сервиса https://jwt.io/.
Передай зашифрованный вариант и убедись, что в «PAYLOAD» там заданные тобой данные.
Добейся того, чтобы появилась надпись «Signature Verified»:
- Алгоритм подписи должен автоматически выставиться в HS256, т.к. он передается в заголовке токена.
- Осталось передать правильный ключ симметричного шифрования в блоке «VERIFY SIGNATURE».


Итак, правильный JWT-токен уже можно получить в виде cookie.
Но пока приложение никак на это не реагирует. Надо это исправить.

Добавь следующий код в `IdentityHostingStartup.cs`:
```cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
        };
    });
```
После этого JWT-токены как-то начнут поддерживаться. Но надо донастроить.

В `TokenValidationParameters`:
1. Выстави `ValidateIssuer` и `ValidateAudience` в `false`,
   потому что эта информация в токен не добавлялась.
2. Выстави `ValidateLifetime` в `true`, чтобы старые токены не работали.
   Также задай `ClockSkew = TimeSpan.Zero`. Дело в том, что токены генерируются и проверяются обычно на разных серверах
   и время на них может отличаться. Поэтому при проверке токенов допускается погрешность в несколько минут.
   Это правильно, но для корректной работы токена с временем жизни в полминуты нужно от погрешности отказаться.
3. Выстави `ValidateIssuerSigningKey` в `true`, чтобы проверялся отпечаток токена.
   В `IssuerSigningKey` передай использованный при создании отпечатка ключ.

Еще один нюанс — откуда будет доставаться токен.
Обычно JWT-токены передаются в заголовке `Authorization` и подписью `Bearer` («на предьявителя»).
Выглядит это примерно так:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.Et9HFtf9R3GEMA0IICOfFMVXY7kkTX1wr4qCyhIf58U
```

Сейчас же токен хранится в cookie. Но можно подсказать ASP.NET Core откуда брать токен вот так:
```cs
options.Events = new JwtBearerEvents
{
    OnMessageReceived = c =>
    {
        c.Token = c.Request.Cookies["NameOfCookieWithToken"];
        return Task.CompletedTask;
    }
};
```
Только не забудь передать правильное имя cookie.


Пришло время убедиться, что все работает:
1. Перейди по секретному адресу `/hack/super_secret_qwe123` и получи токен.
2. Перейди на главную страницу и убедись, что доступка ссылка «Decode»,
   как всем разработчикам.
3. Перейди по ней и убедись, что User заполнен значениями из JWT-токена.
