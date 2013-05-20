using System.Collections.Generic;

namespace PerseusApi.Document{
	public interface IDocumentData : IData{
		List<string> Text { get; set; }
		List<string> Header { get; set; }
		List<DocumentType> Type { get; set; }
	}
}