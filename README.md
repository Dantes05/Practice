**Перед запуском** проекта нужно изменить в appsettings.json поле DefaultConnection, вписать туда ваш сервер, после удалить старые миграции и создать новую.
Открыть консоль диспетчера пакетов из слоя Infrastructure, там писать:
1. Remove-Migration
2. Add-Migration "Название миграции"
3. Update-Database

логин администратора: admin@gmail.com
пароль: Admin123!
Для того, чтобы **авторизоваться** в Swagger нужно выполнить запрос /api/auth/authenticate, введя логин и пароль, при успешно выполнении запроса получите ответ:
{
  "isAuthSuccessful": true,
  "errorMessage": null,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AZ21haWwuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI3NDBiYzdkOS1kZDU1LTRhYzAtODNmMy03MDlkODIwOTY4ZTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsImV4cCI6MTc1MTI4NTgzNywiaXNzIjoiVG9Eb0FwaSIsImF1ZCI6IlRvRG9BcGkifQ.ZRSgjD2PbAuVFoV7BmFTsqwaF4iYKaUDQvPRqlt1uGQ",
  "refreshToken": "JhAIq7EBH33S416QAkehxn+T4AvwCR1dJK6xKWfsoZg="
}
Из этого ответа скопировать token, вверху swagger нажать кнопку Authorize, в открывшемся окне в поле value необходимо вписать Bearer "token"

**Функция восстановления пароля:**
Для восстановления пароля необходимо сначала выполнить запрос /api/auth/forgot-password, введя там email. После этого на email придёт письмо такого вида:
Password Reset Request

To reset your password, use the following token in your API request:

Token: CfDJ8CLXNkAnvZlHtil9SWD1e+YeuxaelcJBkObmQlp0TpGyRfbvOvE/0B0wYJuRaITw3gTzlKkhiPY4y7/I3KLHRCMHcuDRsB21vx9KCdD236CKbT6dXc7giC07sQvASzOgpik6ko2aEl9k+r1Bhiemxcxi+fNpUAGtnWOuvHU+vpRqYgFsmSoRxa5g2rb7xDBUU6+Menh6N9uEeNV79bt1VUyUnaxeoD5WT8uKERRbLiZu
Из этого письма копируем Token, далее выполняем запрос /api/auth/reset-password, вводим email, токен, новый пароль, подтверждаем пароль

При регистрации пользователя можно не указывать реальную почту, подтверждение почты я не реализовал, но при восстановлении пароля необходимо, чтобы вы были зарегестрированы под реальным email
