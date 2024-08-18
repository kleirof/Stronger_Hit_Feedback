using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using System.Reflection;

public class AudioResourceLoader
{

    public static void loadFromFolder(string modName)
    {
        ResourceLoaderSoundbanks LoaderSoundbanks = new ResourceLoaderSoundbanks();
        LoaderSoundbanks.AutoloadFromPath(StrongerHitFeedback.HitFeedbackModule.instance.FolderPath(), modName);
    }
}
