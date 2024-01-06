using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class HitboxManager : SerializedMonoBehaviour
{
    [InfoBox("This is a dictionary that maps the unique identifier of a hitbox to the hitbox controller. " +
             "This is used to retrieve the hitbox controller from the attack data.")]
    [DictionaryDrawerSettings(KeyLabel = "Unique Identifier", ValueLabel = "Hitbox Controller")]
    [SerializeField] [ReadOnly] private List<HitboxEntry> CachedHitboxes;
    private Dictionary<int, HitboxController> _hitboxMap = new Dictionary<int, HitboxController>();

    private void Awake()
    {
        foreach (var entry in CachedHitboxes.Where(entry => !_hitboxMap.TryAdd(entry.Guid, entry.HitboxController)))
        {
            Debug.LogError("Hitbox with uid " + entry.Guid + " already exists in the dictionary." +
                           " \n Did you set it correctly in the attack data or did you forget to add it to the dictionary?");
        }
    }

    public HitboxController GetHitbox(int uid)
    {
        HitboxController hitbox;
        if (!_hitboxMap.TryGetValue(uid, out hitbox))
        {
            Debug.LogError("Hitbox with uid " + uid + " not found." +
                           " \n Did you set it correctly in the attack data or did you forget to add it to the dictionary?");
        }
        return hitbox;
    }

    /// <summary>
    /// Collects all hitboxes in the children of this object and adds them to the dictionary.
    /// </summary>
    [Button("Fetch Hitboxes")]
    private void FetchHitboxes()
    {
        var hitboxes = GetComponentsInChildren<HitboxController>();
        CachedHitboxes = new List<HitboxEntry>();
        foreach (var hitbox in hitboxes)
        {
            if(CachedHitboxes.Exists(x => x.Guid == hitbox.GUID))
            {
                Debug.LogError("Hitbox " + hitbox.name + " has a duplicate GUID. Please set it in the inspector.");
                continue;
            }
            CachedHitboxes.Add(new HitboxEntry(hitbox.GUID, hitbox));
        }
    }
}

[System.Serializable]
public class HitboxEntry
{
    [SerializeField] private int guid;
    public int Guid => guid;
    
    [SerializeField] private HitboxController _hitboxController;
    public HitboxController HitboxController => _hitboxController;
    
    
    public HitboxEntry(int guid, HitboxController hitbox)
    {
        this.guid = guid;
        _hitboxController = hitbox;
    }
}
