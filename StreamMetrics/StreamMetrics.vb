﻿Imports MathNet.Numerics.Statistics
Imports PcapDotNet.Core
Imports System.Text

Module StreamMetrics
    Dim lastTicks As ULong = 0
    Dim deltas As New List(Of Double)

    ' returns the number of characters copied to the buffer, not including the terminating null character.
    ' If supplied destination buffer is too small to hold the requested string, the string is truncated.
    ' In the event the initialization file specified by lpFileName is not found, or contains invalid values,
    ' the Function will set errorno with a value Of '0x2' (File Not Found). To retrieve extended error information, call GetLastError.
    Private Declare Auto Function GetPrivateProfileString Lib "kernel32" (ByVal lpAppName As String,
                ByVal lpKeyName As String,
                ByVal lpDefault As String,
                ByVal lpReturnedString As StringBuilder,
                ByVal nSize As Integer,
                ByVal lpFileName As String) As Integer

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
            Console.Out().WriteLine()

            Dim FilenameInfo As System.IO.FileInfo
            FilenameInfo = My.Computer.FileSystem.GetFileInfo(Filename)
            Dim FileFolderPath As String = FilenameInfo.DirectoryName
            Dim ConfigFilename As String = FileFolderPath & "\StreamMetrics.ini"

            If My.Computer.FileSystem.FileExists(ConfigFilename) Then
                Dim sb As StringBuilder = New StringBuilder(500)

                Dim defaultStrm As ProfessionalMediaStream = New ProfessionalMediaStream()
                defaultStrm.activeHeight = readValueFromIni("default", "active-height", ConfigFilename)
                defaultStrm.activeWidth = readValueFromIni("default", "active-width", ConfigFilename)
                defaultStrm.rate = readValueFromIni("default", "rate", ConfigFilename)
                defaultStrm.interlaced = readValueFromIni("default", "interlaced", ConfigFilename)
                defaultStrm.colorSubsampling = readValueFromIni("default", "color-subsampling", ConfigFilename)
                defaultStrm.sampleWidth = readValueFromIni("default", "sample-width", ConfigFilename)
                defaultStrm.senderType = readValueFromIni("default", "sender-type", ConfigFilename)

                'Override with any specifics defined in INI
                Dim strm As ProfessionalMediaStream = New ProfessionalMediaStream()
                strm.activeHeight = readValueFromIni(FilenameInfo.Name, "active-height", ConfigFilename, defaultStrm.activeHeight)
                strm.activeWidth = readValueFromIni(FilenameInfo.Name, "active-width", ConfigFilename, defaultStrm.activeWidth)
                strm.rate = readValueFromIni(FilenameInfo.Name, "rate", ConfigFilename, defaultStrm.rate)
                strm.interlaced = readValueFromIni(FilenameInfo.Name, "interlaced", ConfigFilename, defaultStrm.interlaced)
                strm.colorSubsampling = readValueFromIni(FilenameInfo.Name, "color-subsampling", ConfigFilename, defaultStrm.colorSubsampling)
                strm.sampleWidth = readValueFromIni(FilenameInfo.Name, "sample-width", ConfigFilename, defaultStrm.sampleWidth)
                strm.senderType = readValueFromIni(FilenameInfo.Name, "sender-type", ConfigFilename, defaultStrm.senderType)

                Console.Out().WriteLine("= ST 2110-21 =")
                Console.Out().WriteLine("Octets to capture the active picture area=" & strm.activeOctets())
                Console.Out().WriteLine("Number of packets per frame of video, N_pkts=" & strm.NPackets())
                Console.Out().WriteLine("Period between consecutive frames of video, T_FRAME (in s)=" & Format(strm.TFrame(), "Scientific"))
                Console.Out().WriteLine("Sender Type=" & strm.senderType)

                Console.Out().WriteLine()
                Console.Out().WriteLine("= Network Compatibility Model Compliance =")
                Console.Out().WriteLine("C_MAX (left part)=" & Format(strm.CMaxSpecLeft(), "General Number"))
                Console.Out().WriteLine("C_MAX (right part)=" & Format(strm.CMaxSpecRight(), "Fixed"))
                Console.Out().WriteLine("C_MAX=" & Format(strm.CMaxSpec(), "General Number"))

                Console.Out().WriteLine()
                Console.Out().WriteLine("= Virtual Receiver Buffer Model Compliance =")
                Console.Out().WriteLine("VRX_FULL (left part)=" & Format(strm.VrxFullSpecLeft(), "General Number"))
                Console.Out().WriteLine("VRX_FULL (right part)=" & Format(strm.VrxFullSpecRight(), "Fixed"))
                Console.Out().WriteLine("VRX_FULL=" & Format(strm.VrxFullSpec(), "General Number"))
            Else
                Console.Out().WriteLine("= StreamMetrics.ini not found, Skipping ST 2110-21 compliance =")
            End If


        Catch e As MissingArgumentException
            Console.Out().WriteLine(e.Message)
        Finally
            Console.Out().WriteLine("Press Enter to continue...")
            Console.ReadLine()
        End Try

    End Sub

    Private Function readValueFromIni(header As String, keyValue As String, filename As String, Optional defValue As String = "") As Object
        Dim sb As StringBuilder = New StringBuilder(500)
        Dim res As Integer = GetPrivateProfileString(header, keyValue, defValue, sb, sb.Capacity, filename)

        ' Only when net in file and defValue is empty
        If res = 0 Then
            Throw New MissingArgumentException(keyValue & " must be defined in [" & header & "] of " & filename)
        End If
        Return sb.ToString
    End Function

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
