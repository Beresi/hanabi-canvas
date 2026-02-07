// ============================================================================
// Copyright (c) 2026 Itay Beresi. All rights reserved.
// ============================================================================
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using HanabiCanvas.Runtime;

namespace HanabiCanvas.Tests.EditMode
{
    public class ListSOTests
    {
        // ---- Private Fields ----
        private GameObjectListSO _collection;
        private List<GameObject> _testObjects;

        // ---- Setup / Teardown ----
        [SetUp]
        public void SetUp()
        {
            _collection = ScriptableObject.CreateInstance<GameObjectListSO>();
            _testObjects = new List<GameObject>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_collection);
            for (int i = 0; i < _testObjects.Count; i++)
            {
                if (_testObjects[i] != null)
                {
                    Object.DestroyImmediate(_testObjects[i]);
                }
            }
            _testObjects.Clear();
        }

        // ---- Helpers ----
        private GameObject CreateTestObject(string name)
        {
            GameObject go = new GameObject(name);
            _testObjects.Add(go);
            return go;
        }

        // ---- Tests ----
        [Test]
        public void Add_NewItem_IncrementsCount()
        {
            GameObject go = CreateTestObject("TestObject");

            _collection.Add(go);

            Assert.AreEqual(1, _collection.Count);
        }

        [Test]
        public void Add_NewItem_FiresOnItemAdded()
        {
            GameObject go = CreateTestObject("TestObject");
            GameObject _receivedItem = null;
            _collection.OnItemAdded += (item) => _receivedItem = item;

            _collection.Add(go);

            Assert.AreEqual(go, _receivedItem);
        }

        [Test]
        public void Remove_ExistingItem_DecrementsCount()
        {
            GameObject go = CreateTestObject("TestObject");
            _collection.Add(go);

            _collection.Remove(go);

            Assert.AreEqual(0, _collection.Count);
        }

        [Test]
        public void Remove_ExistingItem_FiresOnItemRemoved()
        {
            GameObject go = CreateTestObject("TestObject");
            _collection.Add(go);

            GameObject _removedItem = null;
            _collection.OnItemRemoved += (item) => _removedItem = item;

            _collection.Remove(go);

            Assert.AreEqual(go, _removedItem);
        }

        [Test]
        public void Remove_NonexistentItem_ReturnsFalse()
        {
            GameObject go = CreateTestObject("TestObject");

            bool result = _collection.Remove(go);

            Assert.IsFalse(result);
        }

        [Test]
        public void GetAt_ValidIndex_ReturnsCorrectItem()
        {
            GameObject go1 = CreateTestObject("TestObject1");
            GameObject go2 = CreateTestObject("TestObject2");
            _collection.Add(go1);
            _collection.Add(go2);

            Assert.AreEqual(go1, _collection.GetAt(0));
            Assert.AreEqual(go2, _collection.GetAt(1));
        }

        [Test]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            GameObject go = CreateTestObject("TestObject");
            _collection.Add(go);

            Assert.IsTrue(_collection.Contains(go));
        }

        [Test]
        public void Contains_NonexistentItem_ReturnsFalse()
        {
            GameObject go = CreateTestObject("TestObject");

            Assert.IsFalse(_collection.Contains(go));
        }

        [Test]
        public void Clear_WithItems_ResetsCountAndFiresEvent()
        {
            GameObject go1 = CreateTestObject("TestObject1");
            GameObject go2 = CreateTestObject("TestObject2");
            _collection.Add(go1);
            _collection.Add(go2);

            bool _wasClearFired = false;
            _collection.OnCleared += () => _wasClearFired = true;

            _collection.Clear();

            Assert.AreEqual(0, _collection.Count);
            Assert.IsTrue(_wasClearFired);
        }

        [Test]
        public void ProcessAll_WithItems_VisitsAllItems()
        {
            GameObject go1 = CreateTestObject("TestObject1");
            GameObject go2 = CreateTestObject("TestObject2");
            GameObject go3 = CreateTestObject("TestObject3");
            _collection.Add(go1);
            _collection.Add(go2);
            _collection.Add(go3);

            int _counter = 0;
            _collection.ProcessAll((item) => _counter++);

            Assert.AreEqual(3, _counter);
        }

        [Test]
        public void ProcessAll_EmptyCollection_DoesNotInvokeAction()
        {
            int _counter = 0;
            _collection.ProcessAll((item) => _counter++);

            Assert.AreEqual(0, _counter);
        }
    }
}
