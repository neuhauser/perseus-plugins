namespace PerseusApi{
	public interface IDocumentData : IData{
		string[] Text { get; set; }
		DocumentType[] Type { get; set; }
	}
}