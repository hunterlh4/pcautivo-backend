# Omada OC-300 — Estado actual y debug histórico

---

## ✅ ESTADO ACTUAL — FUNCIONANDO

### Fecha de resolución: 4 de marzo de 2026

**El endpoint correcto para autorizar un cliente es:**
```
POST /{controllerId}/api/v2/sites/{siteId}/cmd/clients/{mac}/auth
Headers: Csrf-Token: <admin_token>
         Cookie: TPOMADA_SESSIONID=<session_cookie>
Body: { "time": 28800 }
→ { "errorCode": 0, "msg": "Success." }
```

`extPortal/auth` **NO** es el endpoint correcto — siempre devuelve `-1` independientemente del payload.

---

## Configuración del entorno
- OC-300 IP: `192.168.0.2`
- ControllerId: `ef9a023b5c6bbf1a4dd6ce4e71852664`
- SiteId: `638eff71cbfdfc3b05c3ef36`
- Portal: `Portal Sala` → SSID: `Portal Damasco`
- Tipo de autenticación: **Servidor de portal externo**
- Servidor portal personalizado: `192.168.0.10:5173` (frontend)
- Backend: `192.168.0.10:6011`
- Dispositivo de prueba: Galaxy-A52s-5G, IP: `192.168.0.128`, MAC: `92-67-7E-61-92-23`
- AP MAC: `EC-75-0C-24-27-0A`, RadioId: 1

---

## Flujo completo implementado

```
Dispositivo WiFi → Omada redirect → Frontend (login form)
  → POST /api/captive/login (backend)
      1. Validar credenciales en BD
      2. AuthorizeClientAsync(clientMac, site, ...)
          ├─ POST /api/v2/sites/{siteId}/cmd/clients/{mac}/auth
          │    ├─ errorCode: 0  → ✅ éxito
          │    ├─ 401/403       → renovar token admin y reintentar
          │    └─ errorCode: -41010 (sesión pendiente expiró/reemplazada)
          │         └─ GET /openapi/v2/{controllerId}/sites/{siteId}/clients?mac={mac}
          │              ├─ authStatus >= 1 y active=true → ✅ ya tiene internet
          │              └─ no encontrado / authStatus=0  → ❌ error real
      3. Generar JWT y devolver al frontend
```

### Comportamiento confirma en producción
- Android genera la sesión pendiente (T) al conectarse al WiFi
- El OS puede enviar el login 2-3 veces con distintos T mientras reintenta la detección de portal
- Si T ya expiró (error -41010) pero el cliente ya fue autorizado antes → fallback por OpenAPI lo detecta
- El cliente recibe internet antes de que llegue la respuesta HTTP al frontend (comportamiento normal del OS)

---

## Archivos modificados

### `src/PCautivoCore/Infrastructure/Services/OmadaService.cs`
- `GetAdminTokenAsync()`: Login admin con caché 90 min, devuelve Csrf-Token + guarda cookie TPOMADA_SESSIONID
- `AuthorizeClientAsync()`: Usa `cmd/clients/{mac}/auth`, reintenta hasta 2 veces si token expirado. Fallback -41010 → `IsClientAuthorizedAsync`
- `IsClientAuthorizedAsync()`: GET openapi/v2/.../clients?mac= — verifica `authStatus >= 1 && active`
- `NormMac()`: Convierte MAC a formato `AA-BB-CC-DD-EE-FF` (mayúsculas + guiones)

### `src/PCautivoCore/Infrastructure/Models/Omada/OmadaAuthRequest.cs`
- `OmadaBaseResponse`: `errorCode` + `msg`
- `OmadaClientsResponse` / `OmadaClientsResult` / `OmadaClientInfo`: modelos para la API OpenAPI de clientes

### `src/PCautivoCore/Application/Features/CaptiveAuth/Actions/CaptiveLoginCommand.cs`
- Campos: `Username`, `Password`, `ClientMac`, `ApMac`, `SsidName`, `RadioId`, `T`, `ClientIp`, `Site`, `OriginUrl`
- Log REQUEST al inicio y RESPONSE al final (sin loguear JWT)
- Respuesta exitosa: `CaptiveLoginDto { AccessToken, TokenType, ExpiresIn, Username, LandingUrl }`

### `src/PCautivoCore/Infrastructure/Settings/OmadaSettings.cs`
- `BaseUrl`, `LoginBaseUrl`, `ControllerId`, `Site`, `AdminUsername`, `AdminPassword`, `IgnoreSslErrors`

---

## APIs de Omada confirmadas

| Endpoint | Método | Auth | Resultado |
|---|---|---|---|
| `/{controllerId}/api/v2/login` | POST | ninguna | ✅ devuelve token + cookie |
| `/api/v2/sites/{siteId}/cmd/clients/{mac}/auth` | POST | Csrf-Token + Cookie | ✅ autoriza cliente |
| `/openapi/v2/{controllerId}/sites/{siteId}/clients?mac=` | GET | Csrf-Token + Cookie | ✅ lista clientes con authStatus |
| `/openapi/v1/{controllerId}/sites/{siteId}/clients/search-fields-options` | GET | Csrf-Token + Cookie | ✅ metadatos |
| `/api/v2/hotspot/extPortal/auth?siteId=` | POST | Csrf-Token + Cookie | ❌ siempre -1 |
| `/api/v2/sites/{siteId}/hotspot/clients/{mac}/auth` | POST | Csrf-Token + Cookie | ❌ -1600 Unsupported |

---

## Error codes conocidos de Omada

| Código | Significado | Acción |
|---|---|---|
| `0` | Éxito | — |
| `-1` | Error general (extPortal/auth siempre lo devuelve) | No usar extPortal/auth |
| `-41010` | La sesión pendiente del portal no existe / expiró | Verificar authStatus vía OpenAPI |
| `-1600` | Ruta no soportada | Endpoint incorrecto |

---

## Historial de intentos fallidos (extPortal/auth)

> Se probaron 18+ variaciones del payload en `extPortal/auth`. Todas devolvieron `-1`.
> La conclusión fue confirmada por PowerShell directo (bypaseando el backend): `-1` viene de Omada, no del backend.

| # | campo T | tipo | MACs | time | clientIp | siteId QP | Csrf | Resultado |
|---|---|---|---|---|---|---|---|---|
| 1 | — | — | — | — | no | no | no | 500 (puerto 8043) |
| 4 | — | — | — | — | — | — | no | ✅ login funciona (puerto 443) |
| 6 | — | — | — | — | — | sí | sí | -1600 |
| 7-8 | `token` QS | string | AA-BB | 480 | no | no | sí | -1 |
| 9 | `token` body | string | AA-BB | 480 | no | no | sí | -1 |
| 10 | `token` body | long | AA-BB | 480 | no | no | sí | -1 |
| 11 | `token` body | long | AA-BB | 480 | no | no | **no** | HTML |
| 12 | (como `time`) | long | AA-BB | unix | no | no | sí | -1 |
| 13 | `token` body | long | AA-BB | 28800 | no | no | sí | -1 |
| 14 | `token` body | long | AA-BB | 28800 | no | sí | sí | -1 |
| 15 | `token` body | string | aa:bb | 28800 | no | sí | sí | -1 |
| 16 | `token` body | string | aa:bb | 28800 | sí | sí | sí | -1 |
| 17 | `t` body | string | AA-BB | 28800 | sí | sí | sí | -1 |
| 18 | `t` body | long | AA-BB | 28800 | sí | sí | sí | -1 (PS directo) |

---

## Pendientes / mejoras futuras

- [ ] Evaluar si `time: 28800` (8h) es el valor correcto o si debe venir del panel de Omada
- [ ] Manejar en el frontend el caso de error de red durante el login (el OS destruye la sesión del portal cuando detecta internet)
- [ ] Testear con más de un dispositivo simultáneo para verificar que el singleton de OmadaService y el caché de token funcionen correctamente
- [ ] Revisar si el token admin expira antes de 90 minutos en el OC-300 (ajustar `_tokenExpiry` si es necesario)
