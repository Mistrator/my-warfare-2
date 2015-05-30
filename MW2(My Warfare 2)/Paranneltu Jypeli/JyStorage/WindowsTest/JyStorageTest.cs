using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jypeli;
using System.Windows.Forms;

namespace JyStorageTest
{
    [TestClass]
    public class JyStorageTest
    {
        private IEnumerable<FileManager> EnumerateStorages()
        {
            yield return new WindowsFileManager( WindowsLocation.MyDocuments );
        }

        [TestMethod]
        public void TestCreateExistsRemove()
        {
            const string testFileName = "x0emxujnhp.jhc";

            foreach ( var storage in EnumerateStorages() )
            {
                storage.Delete( testFileName );
                Assert.IsFalse( storage.Exists( testFileName ), "Test file still exists after deletion (#1) for " + storage.GetType().Name );

                StorageFile file = storage.Create( testFileName );
                file.Close();
                Assert.IsTrue( storage.Exists( testFileName ), "Test file does not exist after creation for " + storage.GetType().Name );

                storage.Delete( testFileName );
                Assert.IsFalse( storage.Exists( testFileName ), "Test file still exists after deletion (#2) for " + storage.GetType().Name );
            }
        }

        [TestMethod]
        public void TestMkRmDir()
        {
            const string testDir = "akjfhyshkl";

            foreach ( var storage in EnumerateStorages() )
            {
                storage.RmDir( testDir );
                Assert.IsFalse( storage.Exists( testDir ), "Test directory still exists after deletion (#1) for " + storage.GetType().Name );

                storage.MkDir( testDir );
                Assert.IsTrue( storage.Exists( testDir ), "Test directory does not exist after creation for " + storage.GetType().Name );

                storage.RmDir( testDir );
                Assert.IsFalse( storage.Exists( testDir ), "Test directory still exists after deletion (#2) for " + storage.GetType().Name );
            }
        }

        [Save]
        class TestClass
        {
            [Save] public int Attr1;
            [Save] public char Attr2 { get; set; }
            [Save] internal string NonPublic1;
            [Save] internal string NonPublic2 { get; set; }
            public short DoNotSave1;
            public double DoNotSave2 { get; set; }

            public TestClass( int a1, string np1, string np2 )
            {
                Attr1 = a1;
                NonPublic1 = np1;
                NonPublic2 = np2;
            }

            public string GetNonPublic1() { return NonPublic1; }
            public string GetNonPublic2() { return NonPublic2; }
        }

        [TestMethod]
        public void TestSerialize()
        {
            const string testFileName = "wiropopipoi";
            const short notSaved1 = 9;
            const double notSaved2 = Math.PI;

            TestClass testObj = new TestClass( 47, "C#", "Jypeli" );
            testObj.Attr2 = 'k';
            testObj.DoNotSave1 = 6;
            testObj.DoNotSave2 = 2.41;

            foreach ( var storage in EnumerateStorages() )
            {
                storage.Delete( testFileName );
                Assert.IsFalse( storage.Exists( testFileName ), "Test file still exists after deletion (#1) for " + storage.GetType().Name );

                storage.Save<TestClass>( testObj, testFileName );
                Assert.IsTrue( storage.Exists( testFileName ), "Test file does not exist after creation for " + storage.GetType().Name );

                TestClass loaded = new TestClass( -1, "vanha1", "vanha2" );
                loaded.DoNotSave1 = notSaved1;
                loaded.DoNotSave2 = notSaved2;
                storage.Load<TestClass>( loaded, testFileName );
                Assert.AreEqual( testObj.Attr1, loaded.Attr1, "Attr1 value mismatch in " + storage.GetType().Name );
                Assert.AreEqual( testObj.Attr2, loaded.Attr2, "Attr2 value mismatch in " + storage.GetType().Name );
                Assert.AreEqual( testObj.GetNonPublic1(), loaded.GetNonPublic1(), "NonPublic1 value mismatch in " + storage.GetType().Name );
                Assert.AreEqual( testObj.GetNonPublic2(), loaded.GetNonPublic2(), "NonPublic2 value mismatch in " + storage.GetType().Name );
                Assert.AreEqual( notSaved1, loaded.DoNotSave1, "DoNotSave1 value mismatch in " + storage.GetType().Name );
                Assert.AreEqual( notSaved2, loaded.DoNotSave2, "DoNotSave2 value mismatch in " + storage.GetType().Name );

                storage.Delete( testFileName );
                Assert.IsFalse( storage.Exists( testFileName ), "Test file still exists after deletion (#2) for " + storage.GetType().Name );
            }
        }
    }
}
