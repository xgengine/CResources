using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using H3D.CResources;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class CResourcesLoadTest : IPrebuildSetup
{
    public void Setup()
    {
        Debug.Log("setup");
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        Debug.Log("OneTimeSetup");
    }


    [OneTimeTearDown]
    public void Cleanup()
    {
        Debug.Log("tear down");
    }

    [UnityTest]
    public IEnumerator  CanProvideWithCallback()
    {
        Debug.Log("1");
        yield break;
    }

    [UnityTest]
    public IEnumerator CanProvideWithCallback2()
    {
        Debug.Log("2");
        yield break;

    }
    [UnityTest]
    public IEnumerator  CanProvideWithCallback3()
    {
        Debug.Log("3");
        yield return 1;

        bool result = true;
        Assert.IsTrue(result);
    }
}
