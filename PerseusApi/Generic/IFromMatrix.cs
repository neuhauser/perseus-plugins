﻿using BasicLib.Util;

namespace PerseusApi.Generic{
	public interface IFromMatrix : IActivity{
		string HelpOutput { get; }
		DocumentType HelpOutputType { get; }
		string[] HelpSupplTables { get; }
		DocumentType[] HelpSupplTablesType { get; }
		int NumSupplTables { get; }
		string[] HelpDocuments { get; }
		DocumentType[] HelpDocumentTypes { get; }
		int NumDocuments { get; }
	}
}