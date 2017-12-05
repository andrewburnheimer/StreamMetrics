Imports StreamMetrics

Friend Class ProfessionalMediaStream
    Private Const RTP_PAYLOAD As Integer = 1428 ' per ST 2110
    Private Const MAX_IP As Integer = 1500
    Private lastTicks As ULong = 0
    Public deltas As New List(Of Double)
    Private netCompatBucket As New Queue(Of Long)
    Public netCompatBucketMaxDepth As Integer = 0
    Private virtRecvBuffBucketDepth As Double = 0 ' Must be a float to handle continuous time
    Public virtRecvBuffBucketMaxDepth As Integer = 0
    Public virtRecvBuffBucketMinDepth As Integer = 0

    Public Property activeHeight As Integer
    Public Property activeWidth As Integer
    Public Property colorSubsampling As String
    Public Property interlaced As Boolean
    Public Property rate As Decimal
    Public Property sampleWidth As Integer

    ' Ratio of active time to total time within the frame period
    Public Property rActive As Decimal = 1.0
    Public Property senderType As String = "2110TPW"
    Public Property beta As Decimal = 1.1


    ' Only handles 8, 10, 12, 16 sampleWidths for 4:2:2 and 4:4:4
    Public Function pGroupOctets() As Integer
        Dim octets As Integer = sampleWidth / 2
        If colorSubsampling = "4:4:4" Then
            If sampleWidth = 12 Then
                octets = 9
            Else 'sampleWidth = 16
                octets = 6
            End If
        End If
        Return octets
    End Function

    ' Only handles 8, 10, 12, 16 sampleWidths for 4:2:2 and 4:4:4
    Public Function pGroupPixels() As Integer
        Dim pixels As Integer = 2
        If colorSubsampling = "4:4:4" And sampleWidth = 16 Then
            pixels = 1
        End If
        Return pixels
    End Function

    ' Number of octets to capture the active picture area
    Public Function activeOctets() As Integer
        Return (activeWidth * activeHeight * pGroupOctets()) / pGroupPixels()
    End Function

    ' Number of packets per frame of video (depends on mapping details)
    Public Function NPackets() As Integer
        Return Math.Ceiling(CDbl(activeOctets()) / RTP_PAYLOAD)
    End Function

    ' Period between consecutive frames of video at the prevailing frame rate
    Public Function TFrame() As Decimal
        Dim effRate As Decimal = rate
        If interlaced Then
            effRate = rate / 2
        End If
        Return effRate ^ -1
    End Function

    Public Function TDrain(scaler As Double) As Double
        Return (TFrame() / NPackets()) / scaler
    End Function

    Public Function CMaxSpecLeft() As Double
        If senderType = "2110TPW" Then
            Return 16
        Else
            Return 4
        End If
    End Function

    Public Function CMaxSpecRight() As Double
        Dim ret As Decimal = 0
        If senderType = "2110TPN" Then
            ret = NPackets() / (43200 * rActive * TFrame())
        ElseIf senderType = "2110TPNL" Then
            ret = NPackets() / (43200 * TFrame())
        Else ' 2110TPW
            ret = NPackets() / (21600 * TFrame())
        End If
        Return ret
    End Function

    Public Function CMaxSpec() As Integer
        Return Math.Max(CMaxSpecLeft(), Math.Floor(CMaxSpecRight()))
    End Function

    Public Function VrxFullSpecLeft() As Double
        Dim ret As Decimal = 0
        If senderType Like "2110TPN*" Then
            ret = (1500 * 8) / MAX_IP
        Else ' 2110TPW
            ret = (1500 * 720) / MAX_IP
        End If
        Return ret
    End Function

    Public Function VrxFullSpecRight() As Double
        Dim ret As Decimal = 0
        If senderType Like "2110TPN*" Then
            ret = NPackets() / (27000 * TFrame())
        Else ' 2110TPW
            ret = NPackets() / (300 * TFrame())
        End If
        Return ret
    End Function

    Public Function VrxFullSpec() As Integer
        Return Math.Max(Math.Floor(VrxFullSpecLeft()), Math.Floor(VrxFullSpecRight()))
    End Function

    Public Sub packetEvent(ticks As Long)
        Try
            If lastTicks > 0 Then
                ' deltas converted from ticks (0.1 us) to micro-seconds
                deltas.Add((ticks - lastTicks) / 10)
            End If

            If (Not rate = Nothing) And (Not activeHeight = Nothing) And (Not activeWidth = Nothing) And (Not colorSubsampling = Nothing) And (Not interlaced = Nothing) And (Not sampleWidth = Nothing) Then
                If netCompatBucket.Count > 0 Then
                    Dim bottomOfBucket As Long = netCompatBucket.Peek()
                    Do While bottomOfBucket < (ticks - CInt(TDrain(beta) * 10000000.0))
                        netCompatBucket.Dequeue()
                        If netCompatBucket.Count > 0 Then
                            bottomOfBucket = netCompatBucket.Peek()
                        Else
                            Exit Do
                        End If
                    Loop
                End If

                netCompatBucket.Enqueue(ticks)
                If netCompatBucket.Count > netCompatBucketMaxDepth Then
                    netCompatBucketMaxDepth = netCompatBucket.Count
                End If

                If lastTicks > 0 Then
                    Dim packetsDrained As Double
                    packetsDrained = ((ticks - lastTicks) / 10000000.0) / TDrain(1.0)
                    virtRecvBuffBucketDepth -= packetsDrained

                    virtRecvBuffBucketDepth += 1.0
                    If virtRecvBuffBucketDepth < virtRecvBuffBucketMinDepth Then
                        virtRecvBuffBucketMinDepth = Math.Floor(virtRecvBuffBucketDepth)
                    End If
                    If virtRecvBuffBucketDepth > virtRecvBuffBucketMaxDepth Then
                        virtRecvBuffBucketMaxDepth = Math.Ceiling(virtRecvBuffBucketDepth)
                    End If
                End If
            End If

            lastTicks = ticks
        Catch e As Exception
            Console.Out().WriteLine(e.Message)
        End Try
    End Sub

End Class
