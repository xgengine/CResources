using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Injector;
namespace Build.Pipeline.Interfaces
{
    public class NewPipeline
    {
        [MenuItem("H3D/run")]
        static void run()
        {
            BuildContext context = new BuildContext();
            context.SetContextObject(new FilesItem());
            List<IBuildTask> pipeline = new List<IBuildTask>();
            pipeline.Add(new MyTestTask1());
            pipeline.Add(new MyTestTask2());

            BuildTasksRunner.Run(pipeline, context);
        }

        public class FilesItem: IFilesItem
        {
            private List<string> _files = new List<string>();
            public List<string> filePaths
            {
                get
                {
                    return _files;
                }
            }
        }
        public interface IFilesItem : IContextObject
        {
            List<string> filePaths { get; }
        }


        public class MyTestTask1 : IBuildTask
        {
            [InjectContext]
            IFilesItem filesItem;
            ReturnCode IBuildTask.Run()
            {
 
                for (int i =0; i < 5;i++)
                {
                    filesItem.filePaths.Add(i+" item");
                }

                return ReturnCode.Success;
            }
        }
        public class MyTestTask2 : IBuildTask
        {

            [InjectContext]
            IFilesItem filesItem;
            ReturnCode IBuildTask.Run()
            {
                foreach(var item in filesItem.filePaths)
                {
                    Debug.LogError(item);
                }
                    return ReturnCode.Success;
            }
        }

    }
}
