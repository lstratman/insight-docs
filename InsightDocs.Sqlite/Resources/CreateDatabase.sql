CREATE TABLE Urls
(
	Url TEXT PRIMARY KEY,
	MimeTypeId INTEGER NOT NULL,
	DataIsGzipped INTEGER NOT NULL,
	DataContentLength INTEGER NOT NULL,
	Data BLOB NOT NULL
);

CREATE TABLE MimeTypes
(
	MimeTypeId INTEGER PRIMARY KEY,
	MimeType TEXT NOT NULL
);

CREATE VIRTUAL TABLE Search 
USING fts4
(
	Url, 
	Title,
	Content,
	notindexed=Url
);