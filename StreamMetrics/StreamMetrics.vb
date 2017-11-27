Imports MathNet.Numerics.Statistics
Imports PcapDotNet.Core

Module StreamMetrics
    Dim lastTicks As ULong = 0
    Dim deltas As New List(Of Double)

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

            Dim PacketDevice As OfflinePacketDevice = New OfflinePacketDevice(Filename)
            Dim PacketCommunicator As PacketCommunicator = PacketDevice.Open()

            Using PacketCommunicator
                PacketCommunicator.ReceivePackets(0, AddressOf DispatchPacket)
            End Using

            Console.Out().WriteLine("Read " & deltas.Count + 1 & " packets")
            Console.Out().WriteLine()
            Console.Out().WriteLine("= Summary Statistics =")
            Dim meanSdev As Tuple(Of Double, Double) = Statistics.MeanStandardDeviation(deltas)

            Console.Out().WriteLine("Min interval over file (in us)=" & Format(Statistics.Minimum(deltas), "Fixed"))
            Console.Out().WriteLine("1st percentile (find outliers, in us)=" & Format(Statistics.Percentile(deltas, 1), "Fixed"))
            Console.Out().WriteLine("5th percentile (set standard, in us)=" & Format(Statistics.Percentile(deltas, 5), "Fixed"))
            Console.Out().WriteLine("Median (50th percentile, in us)=" & Format(Statistics.Median(deltas), "Fixed"))
            Console.Out().WriteLine("Average packet interval (in us)=" & Format(meanSdev.Item1, "Fixed"))
            Console.Out().WriteLine("Packet interval standard dev. (in us)=" & Format(meanSdev.Item2, "Fixed"))
            Console.Out().WriteLine("95th percentile (set standard, in us)=" & Format(Statistics.Percentile(deltas, 95), "Fixed"))
            Console.Out().WriteLine("99th percentile (find outliers, in us)=" & Format(Statistics.Percentile(deltas, 99), "Fixed"))
            Console.Out().WriteLine("Max interval over file (in us)=" & Format(Statistics.Maximum(deltas), "Fixed"))
            Console.Out().WriteLine()

            Console.Out().WriteLine("= Packet Interval Numerical Histogram =")
            Dim histogram As Histogram = New Histogram(deltas, 10)

            Dim bucketIdx As Integer = 0
            Do While bucketIdx < histogram.BucketCount
                Console.Out().WriteLine(histogram(bucketIdx).ToString())
                bucketIdx += 1
            Loop
            Console.Out().WriteLine()

            Console.Out().WriteLine("= Packet Interval Pictogram =")
            Console.Out().WriteLine("Width: " & histogram(0).Width)
            WritePictogramToConsole(histogram)

        Catch e As MissingArgumentException
            Console.Out().WriteLine(e.Message)
        Finally
            Console.Out().WriteLine("Press Enter to continue...")
            Console.ReadLine()
        End Try

    End Sub

    Private Sub WritePictogramToConsole(histogram As Histogram, Optional width As Integer = 78)
        Dim bucketIdx As Integer = 0
        Dim bucketMaxCount As Integer = 0
        Do While bucketIdx < histogram.BucketCount
            If histogram(bucketIdx).Count > bucketMaxCount Then
                bucketMaxCount = histogram(bucketIdx).Count
            End If
            bucketIdx += 1
        Loop

        bucketIdx = 0
        Do While bucketIdx < histogram.BucketCount
            Dim axis As String = "(" & histogram(bucketIdx).LowerBound & ";" & histogram(bucketIdx).UpperBound & "]"
            axis = PadString(axis, (width / 4) - 1)
            Console.Out().Write(axis & ":")
            Dim bar As String = WriteBar(histogram(bucketIdx).Count, bucketMaxCount, width - (width / 4))
            Console.Out().Write(bar)
            Console.Out().Write(vbCrLf)
            bucketIdx += 1
        Loop
    End Sub

    Private Function WriteBar(value As Integer, maxValue As Integer, maxLength As Integer) As String
        Dim hashes As String = ""
        hashes = PadString(hashes, (value / CDbl(maxValue)) * maxLength, "#")
        Return hashes
    End Function

    Private Function PadString(str As String, width As Integer, Optional chr As String = " ")
        Dim spaceCount As Integer
        Dim spaces As String = ""
        spaceCount = width - str.Count
        Dim i As Integer = 0
        Do While i < spaceCount
            spaces += chr
            i += 1
        Loop
        Return str & spaces
    End Function

    Private Sub DispatchPacket(packet As PcapDotNet.Packets.Packet)
        If lastTicks > 0 Then
            ' deltas converted from ticks (0.1 us) to micro-seconds
            deltas.Add((packet.Timestamp.Ticks - lastTicks) / 10)
        End If
        lastTicks = packet.Timestamp.Ticks
    End Sub

    Class MissingArgumentException
        Inherits ConstraintException
        Public Sub New(s As String)
            MyBase.New(s)
        End Sub
    End Class

End Module
