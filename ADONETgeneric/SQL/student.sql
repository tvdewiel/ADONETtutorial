CREATE TABLE [dbo].[student]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [naam] NVARCHAR(50) NULL, 
    [cursusId] INT NOT NULL, 
    CONSTRAINT [FK_student_cursus] FOREIGN KEY ([cursusId]) REFERENCES [cursus]([Id])
)
