using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableObjectReferenceCache : ScriptableObject, IExternalStringReferenceResolver
{
    [FolderPath(RequireExistingPath = true)]
    [SerializeField] private string[] foldersToSearchIn;

#if UNITY_EDITOR
    [InlineButton(nameof(ClearReferences))] [InlineButton(nameof(FetchReferences))] [LabelWidth(140)] [PropertySpace(10)]
#endif
    [SerializeField] private bool autoFetchInPlaymode = true;
    
    [ReadOnly]
    [SerializeField] private List<SOCacheEntry> cachedReferences;

    private Dictionary<string, ScriptableObject> guidToSoDict;
    private Dictionary<ScriptableObject, string> soToGuidDict;
    
    [ShowInInspector][HideInEditorMode]
    public bool IsInitialized => guidToSoDict != null && soToGuidDict != null;

    /// <summary>
    /// Populate the dictionaries with the cached references so that they can be retrieved fast for serialization at runtime
    /// </summary>
    public void Initialize()
    {
        if (IsInitialized) return;
        
#if UNITY_EDITOR
        if(autoFetchInPlaymode)
            FetchReferences();
#endif

        guidToSoDict = new Dictionary<string, ScriptableObject>();
        soToGuidDict = new Dictionary<ScriptableObject, string>();
        foreach (var cacheEntry in cachedReferences)
        {
            guidToSoDict[cacheEntry.Guid] = cacheEntry.ScriptableObject;
            soToGuidDict[cacheEntry.ScriptableObject] = cacheEntry.Guid;
        }
    }

#if UNITY_EDITOR
    private void ClearReferences()
    {
        cachedReferences.Clear();
    }
    
    /// <summary>
    /// Searches for all scriptable objects that implement ISerializeReferenceByAssetGuid or ISerializeReferenceByAssetGuid and saves them in a list together with their guid
    /// </summary>
    private void FetchReferences()
    {
        cachedReferences = new List<SOCacheEntry>();
        
        var assetGuidTypes = GetSoTypesWithInterface<ISerializeReferenceByAssetGuid>();
        var instancesWithAssetGuid = GetAssetsOfTypes<ISerializeReferenceByAssetGuid>(assetGuidTypes, foldersToSearchIn);
        foreach (var scriptableObject in instancesWithAssetGuid)
        {
            var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scriptableObject));
            cachedReferences.Add(new SOCacheEntry(assetGuid, scriptableObject));
        }
        
        var customGuidTypes = GetSoTypesWithInterface<ISerializeReferenceByCustomGuid>();
        var instancesWithCustomGuid = GetAssetsOfTypes<ISerializeReferenceByCustomGuid>(customGuidTypes, foldersToSearchIn);
        foreach (var scriptableObject in instancesWithCustomGuid)
        {
            var guid = ((ISerializeReferenceByCustomGuid) scriptableObject).Guid;
            cachedReferences.Add(new SOCacheEntry(guid, scriptableObject));
        }
    }

    /// <summary>
    /// Get all types that derive from scriptable object and implement interface T
    /// </summary>
    private List<Type> GetSoTypesWithInterface<T>()
    {
        return AssemblyUtilities.GetTypes(AssemblyTypeFlags.All)
            .Where(t =>
                !t.IsAbstract &&
                !t.IsGenericType &&
                typeof(T).IsAssignableFrom(t) &&
                t.IsSubclassOf(typeof(ScriptableObject)))
            .ToList();
    }
    
    /// <summary>
    /// Returns all scriptable objects that are of one of the passed in types and implement T as well.
    /// </summary>
    /// <param name="searchInFolders"> Optionally limit the search to certain folders </param>
    private List<ScriptableObject> GetAssetsOfTypes<T>(IEnumerable<Type> types, params string[] searchInFolders)
    {
        return types
            .SelectMany(type =>
                AssetDatabase.FindAssets($"t:{type.Name}", searchInFolders))
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
            .Where(scriptableObject => scriptableObject is T) // make sure the scriptable object implements the interface T because AssetDatabase.FindAssets might return wrong assets if types of different namespaces have the same name
            .ToList();
    }
#endif

    #region Members of IExternalStringReferenceResolver
    public bool CanReference(object value, out string id)
    {
        EnsureInitialized();
        
        id = null;
        if (!(value is ScriptableObject so))
            return false;
        
        if (!soToGuidDict.TryGetValue(so, out id))
            id = "not_in_database";
        
        return true;
    }
    
    public bool TryResolveReference(string id, out object value)
    {
        EnsureInitialized();
        
        value = null;
        if (id == "not_in_database") return true;
        
        var containsId = guidToSoDict.TryGetValue(id, out var scriptableObject);
        value = scriptableObject;
        return containsId;
    }

    public IExternalStringReferenceResolver NextResolver { get; set; }
    
    private void EnsureInitialized()
    {
        if (IsInitialized) return;
        
        Initialize();
        Debug.LogWarning($"Had to initialize {nameof(ScriptableObjectReferenceCache)} lazily because it wasn't initialized before use!");
    }
    #endregion
}

[Serializable]
public class SOCacheEntry
{
    [SerializeField] private string guid;
    public string Guid => guid;
    
    [SerializeField] private ScriptableObject scriptableObject;
    public ScriptableObject ScriptableObject => scriptableObject;
    
    
    public SOCacheEntry(string guid, ScriptableObject scriptableObject)
    {
        this.guid = guid;
        this.scriptableObject = scriptableObject;
    }
}

public interface ISerializeReferenceByAssetGuid
{
}

public interface ISerializeReferenceByCustomGuid
{
    string Guid { get; }
}