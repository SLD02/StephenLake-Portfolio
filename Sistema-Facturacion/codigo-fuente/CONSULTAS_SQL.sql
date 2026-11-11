-- CONSULTAS DE VERIFICACIÓN PARA SQL SERVER MANAGEMENT STUDIO
-- Primero ejecuta el programa para crear la base de datos y registrar datos.
USE FacturacionTurnosDB;
GO
SELECT Id, FullName, Username, Role, Shift, IsActive FROM dbo.Users;
SELECT Id, Code, Name, UnitPrice, Stock FROM dbo.Products;
SELECT Id, Number, IssuedAt, PaymentMethod, GrossTotal, ItbisTotal, AmountDue,
       AmountPaid, Change, CashierId, CustomerId FROM dbo.Invoices ORDER BY Id DESC;
SELECT ProductCode, ProductName, Quantity, UnitPrice, Gross, Itbis, Total FROM dbo.InvoiceLines;
SELECT Id, InvoiceId, CustomerId, OriginalAmount, Balance FROM dbo.Receivables;
SELECT Id, ReceivableId, Amount, CollectedAt FROM dbo.CollectionPayments;
SELECT Id, NextNumber FROM dbo.InvoiceSequences;
