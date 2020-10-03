using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WhatsInTheBox;

public class AudioMan : MonoBehaviour
{
    [SerializeField] GameEvent exitManagement;
    // Start is called before the first frame update

    public void destroyItSelf() {
        Destroy(this.gameObject);
    }
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        exitManagement.AddListener(destroyItSelf);
    }

    void OnDestroy() {
        exitManagement.RemoveListener(destroyItSelf);
    }
}
