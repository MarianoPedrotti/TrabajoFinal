# proyecto-exchange

Pequeño frontend para probar compra/venta de criptomonedas contra un backend .NET.

**Requisitos**
- Node 18+ (para correr Vite)
- .NET 8 (para el backend)

**Instalación**
```bash
cd proyecto-Exchange
npm install
```

**Variables de entorno**
Crea un archivo `.env.local` en la raíz de `proyecto-Exchange` con la URL del backend:
```
VITE_API_URL=https://localhost:7149
```
Cambia el puerto según el backend que uses (`Final`, `TrabajoPractico` o `webApi`).

**Ejecutar en desarrollo**
1. Arrancar el backend (desde Visual Studio o con dotnet run desde la solución correspondiente).
2. Iniciar el frontend:
```bash
cd proyecto-Exchange
npm run dev
```
3. Abrir `http://localhost:5173` (o el puerto que muestre Vite).

**Notas y troubleshooting**
- Si ves errores CORS en la consola del navegador, habilita el origen `http://localhost:5173` en la política CORS del backend (archivo `Program.cs`) o ejecuta backend y frontend en el mismo origen en desarrollo.
- Si ves error de certificado (ERR_CERT_AUTHORITY_INVALID) en HTTPS, puedes:
  - Aceptar el certificado de desarrollo en el navegador, o
  - Configurar el backend para servir HTTP en desarrollo temporalmente.

**Diagnóstico de cotizaciones**
- El componente `src/view/CompraVenta.vue` muestra un panel `Diagnóstico cotización` con el `raw data` que devuelve la API y el precio elegido. Esto ayuda a depurar por qué algunas monedas aparecen como `N/D` o con valores inesperados.
- La store `src/store/monedas.js` usa `VITE_API_URL` para obtener la lista de monedas.

**Cambios relevantes**
- `src/store/monedas.js`: uso de `VITE_API_URL` y validación de respuesta.
- `src/view/CompraVenta.vue`: usa la store, protege accesos nulos y añade heurística y panel diagnóstico para cotizaciones.

Si querés, puedo agregar instrucciones para ejecutar tests (si se añaden) o un script de `start` que levante el backend y frontend simultáneamente.
