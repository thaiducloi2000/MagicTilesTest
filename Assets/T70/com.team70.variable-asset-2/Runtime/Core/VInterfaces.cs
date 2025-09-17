namespace nano.va2
{

    // DATA CLASSIFICATION
    


   
    

    // STRUCTURAL
    public interface IParentAsset
    {
        #if UNITY_EDITOR
        VContainer GetContainer();
        bool DestroyChild(VAsset child);
        #endif
        
        void OnChildAssetChange(VAsset child);
        void CopyChangesToChildren();
    }
    
    public interface  IInit { void Init(); }
    
    // IMPORT - EXPORT
    public interface ISupportJson
    {
        bool FromJson(string json);
        string ToJson();
    }

    public interface ISupportTSV
    {
        bool FromTSV(string tsv);
        string ToTSV();
    }

	public interface ISupportTSV2 : ISupportTSV
    {
        bool FromTSV(string tsv, int st, int ed);
    }

    public interface IPrimityT : ISupportJson { }
    public interface IClassAssetT : ISupportJson { }
    public interface IListAssetT {}
	
    public interface IFloat { float floatValue { get; } }
    public interface IInt { int intValue { get; } }
}