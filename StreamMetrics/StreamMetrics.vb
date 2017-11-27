Module StreamMetrics
    Dim Count As Integer = 0

    Sub Main()

        Try
            Dim Filename As String = ""
            Dim args = My.Application.CommandLineArgs
            If args.Count() > 0 Then
                Filename = args.Item(args.Count() - 1).ToString
                Console.Out().WriteLine("Reading: " & Filename)
            Else
                Throw New MissingArgumentException("Please run on console with a filename argument, or drag and drop an input file on icon or shortcut.")
            End If

            Dim PacketDevice As PcapDotNet.Core.OfflinePacketDevice = New PcapDotNet.Core.OfflinePacketDevice(Filename)
            Dim PacketCommunicator As PcapDotNet.Core.PacketCommunicator = PacketDevice.Open()

            Using PacketCommunicator
                PacketCommunicator.ReceivePackets(0, AddressOf DispatcherHandler)
            End Using

            Console.Out().WriteLine("Read " & Count & " packets")

        Catch e As MissingArgumentException
            Console.Out().WriteLine(e.Message)
        Finally
            Console.Out().WriteLine("Press Enter to continue...")
            Console.ReadLine()
        End Try

    End Sub

    Sub DispatcherHandler(packet As PcapDotNet.Packets.Packet)
        Count += 1
        Console.Out().Write(".")
    End Sub

    Class MissingArgumentException
        Inherits ConstraintException
        Public Sub New(s As String)
            MyBase.New(s)
        End Sub
    End Class

End Module
