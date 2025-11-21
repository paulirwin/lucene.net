The goal is to port code from the OpenNLP project into this library.
The code should go into the Upstream folder. This folder is assumed to have a root namespace of Opennlp.Tools.
Use JavaToCSharpCli to port the code from Java to C#. This tool is available on the path.
It can be used like this to output the converted code to standard output (which can then be redirected to a file):
```
JavaToCSharpCli file path/to/source/File.java
```

The OpenNLP code is available at ~/git/opennlp/opennlp-tools/src/main/java/opennlp/tools

When porting the code requested, just find the Java file on disk, run the JavaToCSharpCli tool on it, and then copy the output into a new file in the Upstream folder with the same relative path and filename, but with a .cs extension instead of .java. Use a capital letter for the first letter of each folder and filename, as is standard in C#.
- Please remove unused using directives after converting, especially the Java ones.
- Please fix up any issues from the conversion after you write the file. Make these as separate edits for approval. Examples include changing Element to XmlElement, or Integer.ParseInt to int.Parse.
- Use HashCode.Combine instead of Arrays.GetHashCode
- Comment out any serialization methods. We only support inference of existing models, not serializing new models.