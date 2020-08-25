# Mailservice
The Mail Service API provides an external email and anti-spam layer for front end only websites that are deployed in environments  that do not provide support for back end systems.

## Client Usage
**Open an [Issue](./issues/new) to request access to this service** - Only allowed hosts can send emails using the mail-service.

#### Check the services is available
Before displaying an email for it is recommended that you hit the GET endpoint to see if the service is available and accepting connections. `https://mailservice.samuelgrant.dev/`

If the service is online and accepting connections an `HTTP 200` will be returned with the following JSON payload
```json
{
	"ApiStatus":"Running",
    "Since":"2/07/2020 8:17:57 am",
    "API Version":"0.0.1.0",
    "reCaptchaKey":"<public key>"
}
```
| Field | Notes |
| ---- | ---- |
| reCaptchaKey | The public key for reCaptcha v2 (tick box) validation. If this field provides a site key you **must** use it to challenge the user and POST the code with the email data in the **POST /** requests.
     
     
#### Send an Email
To send an email make an HTTP request to `POST /` with the following endpoint, all fields are required except for CaptchaCode which is only required if the `GET /` endpoint serves a `reCaptchaKey`.
```json
{
    "CaptchaCode": "",
    "From": {
       "Name": "",
       "Email": ""
    },
    "Message": "",
    "Subject": ""
}
```
| Feild       | Notes                                         |
| ----------  | --------------------------------------------- |
| From.Name   | The sender's name  (Required)                  |
| From.Email  | The sender's email (Required)                  |
| Message     | The message body, **must be HTML** (Required) |
| Subject     | The message subject                           |
| CaptchaCode | The verification code from Google reCaptcha - (Only required if the `GET /` endpoint returns a reCaptchaKey |