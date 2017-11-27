Imports StreamMetrics

Friend Class ProfessionalMediaStream
    Private Const RTP_PAYLOAD As Integer = 1428
    Private Const MAX_IP As Integer = 1500

    Public Property activeHeight As Integer
    Public Property activeWidth As Integer
    Public Property colorSubsampling As String
    Public Property interlaced As Boolean
    Public Property rate As Decimal
    Public Property sampleWidth As Integer
    ' Ratio of active time to total time within the frame period
    Public Property rActive As Decimal

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
End Class