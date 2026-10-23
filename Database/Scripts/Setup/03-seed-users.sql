USE BeeMart;
GO

IF NOT EXISTS (SELECT 1 FROM MsUser WHERE Username = 'admin')
    INSERT INTO MsUser (IdUser, Username, FullName, PasswordHash, Role, Stsrc, Datein, Userin)
    VALUES ('93837', 'admin', 'Administrator',
            '$2a$11$4HVoCOVVTiLkxeaziJhdHOcl.8A0Fb37gnHFLiF2ySzaOG.Uf1q6W',
            'Admin', 1, SYSUTCDATETIME(), 'SYSTEM');

IF NOT EXISTS (SELECT 1 FROM MsUser WHERE Username = 'kasir1')
    INSERT INTO MsUser (IdUser, Username, FullName, PasswordHash, Role, Stsrc, Datein, Userin)
    VALUES ('53977', 'kasir1', 'Kasir Satu',
            '$2a$11$NmrmqZ2yeMtjIjq6UAf2..FA9ZeAqtuMoR0.7yCJoSrwLv2fJvKPO',
            'Cashier', 1, SYSUTCDATETIME(), 'SYSTEM');
GO
