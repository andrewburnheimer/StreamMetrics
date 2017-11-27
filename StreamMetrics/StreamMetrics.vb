Module StreamMetrics

    Sub Main()
        Try
            Dim Filename As String = ""
            Dim args = My.Application.CommandLineArgs
            If args.Count() > 0 Then
                Filename = args.Item(0).ToString
            Else
                Throw New MissingArgumentException("Please run on console with a filename argument, or drag and drop an input file on icon or shortcut.")
            End If

            Dim ReadHandle As IO.StreamReader = FileIO.FileSystem.OpenTextFileReader(Filename)
            'could be a big file...
            Dim FileText As String = ReadHandle.ReadToEnd()
            ReadHandle.Close()

        Catch e As MissingArgumentException
            Console.Out().WriteLine(e.Message)
        Finally
            Console.Out().WriteLine("Press Enter to continue...")
            Console.ReadLine()
        End Try

    End Sub
    Class MissingArgumentException
        Inherits ConstraintException
        Public Sub New(s As String)
            MyBase.New(s)
        End Sub
    End Class

End Module
