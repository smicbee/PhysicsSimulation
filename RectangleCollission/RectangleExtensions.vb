Imports System.Runtime.CompilerServices

Module RectangleExtensions
    <Extension()>
    Function MidX(ByVal rect As Rectangle) As Integer
        Return rect.Left + rect.Width / 2
    End Function

    <Extension()>
    Function MidY(ByVal rect As Rectangle) As Integer
        Return rect.Top + rect.Height / 2
    End Function

    <Extension()>
    Function Center(ByVal rect As Rectangle) As Point
        Return New Point(rect.MidX(), rect.MidY())
    End Function

    <Extension()>
    Function MidX(ByVal rect As RectangleF) As Single
        Return rect.Left + rect.Width / 2
    End Function

    <Extension()>
    Function MidY(ByVal rect As RectangleF) As Single
        Return rect.Top + rect.Height / 2
    End Function

    <Extension()>
    Function Center(ByVal rect As RectangleF) As PointF
        Return New PointF(rect.MidX(), rect.MidY())
    End Function
End Module
