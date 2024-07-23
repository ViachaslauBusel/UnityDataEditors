using ObjectRegistryEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class GenericTestSelectObject<T> : MonoBehaviour where T : IDataObject
{
    [SerializeField]
    private T _dataObject;
}

