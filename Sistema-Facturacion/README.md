# 🧾 Sistema de Facturación — C# y SQL Server

**Tipo:** proyecto académico · **Plataforma:** Windows · **Tecnologías:** C#, Windows Forms, .NET 8, Entity Framework Core y SQL Server.

## Descripción

Aplicación de escritorio para gestionar productos, clientes, cajeros, facturas y reportes. Usa base de datos relacional y un inicio de sesión con roles.

## Funcionalidades

- Inicio de sesión de administrador y cajeros.
- Administración de productos, clientes y cuentas de usuario.
- Registro de facturas y cálculo de impuestos.
- Ventas al contado y a crédito.
- Impresión y reimpresión de facturas.
- Consultas y reportes de ventas e inventario.

## Capturas de pantalla

Pendientes de añadir. Usa los datos de prueba indicados en la [guía de capturas](imagenes/LEEME_CAPTURAS.md).

## Cómo ejecutarlo

1. Instala **Visual Studio 2022** con desarrollo de escritorio .NET.
2. Instala **.NET 8 SDK** y SQL Server Express o LocalDB.
3. Abre `codigo-fuente/SistemaFacturacionWinForms.sln`.
4. Restaura los paquetes y presiona **F5**.
5. Configura tu servidor SQL Server y crea la cuenta administradora inicial.

**Nota:** el código procede de una versión archivada del proyecto. No se ha recompilado ni probado aquí. No utilices información de clientes reales al mostrarlo.

## Organización

- [`codigo-fuente/Forms/`](codigo-fuente/Forms/) — interfaz y formularios
- [`codigo-fuente/Services/`](codigo-fuente/Services/) — lógica de facturación y seguridad
- [`codigo-fuente/Data/`](codigo-fuente/Data/) — conexión y persistencia
- [`codigo-fuente/Models/`](codigo-fuente/Models/) — entidades
