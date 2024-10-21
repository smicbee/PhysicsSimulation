

Imports System.ComponentModel
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices




Public Class UpdateRenderEventArgs
    Inherits EventArgs
    Public outrender As Bitmap
End Class

Public Class LiveTimeExpiredEventArgs
    Inherits EventArgs
    Public PhysObj As Simulation.PhysObj
End Class

Public Class Simulation

    <DllImport("gdi32.dll")>
    Public Shared Function BitBlt(ByVal hdcDest As IntPtr, ByVal nXDest As Integer, ByVal nYDest As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hdcSrc As IntPtr, ByVal nXSrc As Integer, ByVal nYSrc As Integer, ByVal dwRop As Integer) As Boolean

    End Function



    Public Shared objectcounterid As Integer = -1
    Public backgroundcolor As Color = Color.White

    Public Shared Function getcounter() As Integer
        Simulation.objectcounterid += 1
        Return objectcounterid
    End Function



    Public Event RenderUpdated(ByVal sender As Object, e As UpdateRenderEventArgs)
    Public Event PhysObjLivetimeExpired(ByVal sender As Object, e As LiveTimeExpiredEventArgs)

    Public Sub New(render_size As Size)
        Me.windowsize = render_size
    End Sub

    Public Sub Start()
        AddHandler renderthread.DoWork, AddressOf render_next_frame
        AddHandler renderthread.ProgressChanged, AddressOf update_render
        renderthread.WorkerReportsProgress = True
        renderthread.RunWorkerAsync()

    End Sub
    Dim collisionobjects As New List(Of PhysObj)
    Dim ellapsedticks As Integer = 0
    Dim FPS As Integer = 30
    Dim windowsize As Size

    Dim gravity As PointF = New PointF(0.1, 0.0)
    'Dim gravity As PointF = New PointF(0, 0)
    Dim friction As PointF = New PointF(0.01, 0.01)
    'Dim friction As PointF = New PointF(0.00, 0.000)






    Class PhysObj
        Public Sub Dispose()
            img.Dispose()

        End Sub
        Public Sub New(position As RectangleF, movementspeed As PointF, Optional charge As Decimal = 0, Optional mass As Decimal = 1, Optional does_move As Boolean = True, Optional bouncyness As Decimal = 1, Optional img As Image = Nothing, Optional imggif_FPS As Integer = 0, Optional despawntime As DateTime = Nothing)
            Me.position = position
            Dim pencolor As Color
            Dim bordercolor As Color


            Dim maxcharge As Integer = 50

            If charge > 0 Then
                pencolor = Color.Red
                bordercolor = Color.Red
                If Math.Abs(charge) < maxcharge Then
                    pencolor = Color.FromArgb(Math.Abs(charge) / maxcharge * 255, pencolor)
                End If
            ElseIf charge < 0 Then
                pencolor = Color.Blue
                bordercolor = Color.Blue
                If Math.Abs(charge) < maxcharge Then
                    pencolor = Color.FromArgb(Math.Abs(charge) / maxcharge * 255, pencolor)
                End If
            Else
                bordercolor = Color.Black
                pencolor = Nothing
            End If


            Me.fillcolor = pencolor
            Me.border = New Pen(bordercolor, 2)
            Me.img = img
            Me.speed = movementspeed
            Me.id = getcounter()
            Me.charge = charge
            Me.mass = mass
            Me.does_move = does_move
            Me.bouncyness = bouncyness
            Me.gifFPS = imggif_FPS
            Me.activegifframe = 0
            Me.despawntime = despawntime
        End Sub

        Public gifFPS As Integer
        Public img As Image
        Public activegifframe As Integer
        Public bouncyness As Decimal
        Public does_move As Boolean
        Public fillcolor As Color
        Public id As Integer
        Public position As RectangleF
        Public speed As PointF
        Public border As Pen
        Public charge As Decimal
        Public mass As Decimal
        Public despawntime As DateTime
        Public selected As Boolean = False
        Public collideswithwalls As List(Of CollisionWalls) = {CollisionWalls.WallYTop, CollisionWalls.WallXLeft, CollisionWalls.WallXRight, CollisionWalls.WallYBottom}.ToList
    End Class

    Public Enum CollisionWalls
        WallXLeft = 0
        WallXRight = 1
        WallYTop = 2
        WallYBottom = 3
    End Enum

    Dim renderthread As New BackgroundWorker


    Public Function Mensch(offsetpt As Point) As CompositeObjekt

        Dim linkerfusspos As New Rectangle(100 + offsetpt.X, 100 + offsetpt.Y, 20, 50)
        Dim rechterfusspos As New Rectangle(200 + offsetpt.X, 100 + offsetpt.Y, 20, 50)

        Dim bauch As New Rectangle(100 + offsetpt.X, 50 + offsetpt.Y, 120, 50)
        Dim kopf As New Rectangle(132 + offsetpt.X, 0 + offsetpt.Y, 50, 50)





        Dim kopfobj As New PhysObj(kopf, Nothing, 0, 10000)
        Dim bauchobj As New PhysObj(bauch, Nothing, 0, 10000)
        Dim linkerfuss As New PhysObj(linkerfusspos, Nothing, 0, 10000)
        Dim rechterfuss As New PhysObj(rechterfusspos, Nothing, 0, 10000)


        Dim returnobj As CompositeObjekt = New CompositeObjekt({kopfobj, bauchobj, linkerfuss, rechterfuss})

        Return returnobj
    End Function

    Public Function Blackhole(offsetpt As Point) As CompositeObjekt

        Dim singularitypos As New Rectangle(offsetpt.X, offsetpt.Y, 2, 2)

        Dim singularity As New PhysObj(singularitypos, Nothing, 0, 100)


        Dim returnobj As CompositeObjekt = New CompositeObjekt({singularity})

        Return returnobj
    End Function



    Public Function ChristmasGIF(offsetpt As Point, Optional ByVal scale As Decimal = 1) As CompositeObjekt

        Dim gif As Image = Image.FromFile("B:\Google Drive\Desktop\christmas-holiday.gif")


        Dim singularitypos As New Rectangle(offsetpt.X, offsetpt.Y, gif.Width * scale, gif.Height * scale)

        Dim singularity As New PhysObj(singularitypos, Nothing, 0, 100, img:=gif)
        singularity.border = Nothing

        Dim returnobj As CompositeObjekt = New CompositeObjekt({singularity})

        Return returnobj
    End Function




    Class CompositeObjekt
        Public Sub New(objs As PhysObj())
            singles = objs
        End Sub

        Public singles As PhysObj()
    End Class

    Public Function collideswithwall_x(obj As PhysObj)

        If obj.position.Left < 0 AndAlso obj.collideswithwalls.Contains(CollisionWalls.WallXLeft) Then
            obj.position.X = 0
            Return True
        End If

        If obj.position.Right > windowsize.Width AndAlso obj.collideswithwalls.Contains(CollisionWalls.WallXRight) Then
            obj.position.X = windowsize.Width - obj.position.Width
            Return True
        End If


        Return False
    End Function

    Public Function collideswithwall_y(obj As PhysObj)

        If obj.position.Top < 0 AndAlso obj.collideswithwalls.Contains(CollisionWalls.WallYTop) Then
            obj.position.Y = 0
            Return True
        End If

        If obj.position.Bottom > windowsize.Height AndAlso obj.collideswithwalls.Contains(CollisionWalls.WallYBottom) Then
            obj.position.Y = windowsize.Height - obj.position.Height
            Return True
        End If

        Return False
    End Function


    Public Function collides(obj1 As PhysObj, obj2 As PhysObj) As Boolean

        If obj1.position.IntersectsWith(obj2.position) Then
            Return True
        End If


        Return False
    End Function

    Public Function collideswithanyotherobj(obj As PhysObj) As PhysObj()
        Dim collissionlist As New List(Of PhysObj)

        For Each obj2 In collisionobjects

            If obj.id = obj2.id Then
                Continue For
            End If


            If collides(obj, obj2) Then
                collissionlist.Add(obj2)
            End If


        Next


        Return collissionlist.ToArray

    End Function

    Sub apply_speed(obj As PhysObj)

        If obj.does_move Then
            obj.position.X += obj.speed.X
            obj.position.Y += obj.speed.Y
        End If

    End Sub

    Dim gravity_between_objects As Boolean = False

    Sub apply_gravity(obj As PhysObj)
        obj.speed.X += gravity.X
        obj.speed.Y += gravity.Y

        If gravity_between_objects Then


            For Each item In collisionobjects

                If item.id = obj.id Then
                    Continue For
                End If


                Dim r As Decimal = Math.Sqrt((obj.position.X - item.position.X) ^ 2 + (obj.position.Y - item.position.Y) ^ 2)

                Dim chargesquared As Decimal = item.mass * obj.mass / 1000

                Dim F As Decimal = -chargesquared / r ^ 2 / obj.mass


                Dim angle As Decimal = Math.Atan2((obj.position.MidX - item.position.MidX), (obj.position.MidY - item.position.MidY))

                'item.speed.X += Math.Sin(angle) * F
                Dim forcex As Decimal = Math.Sin(angle) * F
                obj.speed.X += forcex

                Dim forcey As Decimal = Math.Cos(angle) * F
                obj.speed.Y += forcey



            Next
        End If

    End Sub

    Sub apply_friction(obj As PhysObj)

        If obj.speed.X > 0 Then
            obj.speed.X -= friction.X
        Else
            obj.speed.X += friction.X
        End If

        If obj.speed.Y > 0 Then
            obj.speed.Y -= friction.Y
        Else
            obj.speed.Y += friction.Y
        End If



    End Sub

    Sub apply_charge_rejection(obj As PhysObj)

        If obj.charge = 0 Then
            Exit Sub
        End If


        For Each item In collisionobjects

            If item.id = obj.id OrElse item.charge = 0 Then
                Continue For
            End If


            Dim r As Decimal = Math.Sqrt((obj.position.X - item.position.X) ^ 2 + (obj.position.Y - item.position.Y) ^ 2)

            Dim chargesquared As Decimal = item.charge * obj.charge

            Dim F As Decimal = chargesquared / r ^ 2 / obj.mass


            Dim angle As Decimal = Math.Atan2((obj.position.MidX - item.position.MidX), (obj.position.MidY - item.position.MidY))

            'item.speed.X += Math.Sin(angle) * F
            Dim forcex As Decimal = Math.Sin(angle) * F
            obj.speed.X += forcex

            Dim forcey As Decimal = Math.Cos(angle) * F
            obj.speed.Y += forcey



        Next


    End Sub

    Public Sub Remove_PhysObj(obj As PhysObj)
        objstoremove.Add(obj)
    End Sub

    Sub apply_despawning(obj As PhysObj, now As DateTime)
        If Not (obj.despawntime = Nothing) AndAlso Not obj.selected Then

            If now > obj.despawntime Then
                'objstoremove.Add(obj)
                RaiseEvent PhysObjLivetimeExpired(obj, New LiveTimeExpiredEventArgs())
            End If

        End If
    End Sub

    Dim objstoremove As New List(Of PhysObj)
    Dim objstoadd As New List(Of PhysObj)




    Sub render_next_frame(sender As Object, e As DoWorkEventArgs)
        Dim outrender As New Bitmap(windowsize.Width, windowsize.Height)
        Dim g As Graphics = Graphics.FromImage(outrender)
        Dim bck As BackgroundWorker = sender
        Dim fpscalc As New Stopwatch
        Dim effectivefps As New Stopwatch


        While True
            Dim addlistdup As New List(Of PhysObj)
            addlistdup = objstoadd.ToList
            objstoadd.Clear()

            For Each obj In addlistdup
                collisionobjects.Add(obj)
            Next

            For Each obj In objstoremove
                collisionobjects.Remove(obj)
                obj.Dispose()
            Next
            objstoremove.Clear()

            Dim now As DateTime = DateTime.Now
            effectivefps.Reset()
            fpscalc.Reset()

            effectivefps.Start()
            fpscalc.Start()

            g.Clear(backgroundcolor)


            For Each obj In collisionobjects
                apply_despawning(obj, now)
                apply_friction(obj)
                apply_gravity(obj)
                apply_speed(obj)
                apply_charge_rejection(obj)

                If collideswithwall_x(obj) Then
                    obj.speed.X = -obj.speed.X
                    obj.position.X += obj.speed.X
                    obj.speed.X = obj.speed.X * obj.bouncyness
                End If


                If collideswithwall_y(obj) Then
                    obj.speed.Y = -obj.speed.Y
                    obj.position.Y += obj.speed.Y
                    obj.speed.Y = obj.speed.Y * obj.bouncyness
                End If

                Dim collideswith As PhysObj() = collideswithanyotherobj(obj)

                For Each collobj In collideswith
                    ResolveCollision(obj, collobj)
                Next

                Dim roundedrectangle As Rectangle = New Rectangle((obj.position.X), (obj.position.Y), (obj.position.Width), (obj.position.Height))

                If obj.border IsNot Nothing Then
                    g.DrawRectangle(obj.border, roundedrectangle)
                End If
                If obj.img IsNot Nothing Then
                    Dim framedimensions As Imaging.FrameDimension = New FrameDimension(obj.img.FrameDimensionsList(0))
                    obj.img.SelectActiveFrame(framedimensions, ellapsedticks Mod obj.img.GetFrameCount(framedimensions))
                    Dim pt As New Point(obj.position.X, obj.position.Y)

                    'BitBlt implementation
                    Dim hdcDest As IntPtr = g.GetHdc
                    Dim s As Graphics = Graphics.FromImage(obj.img)
                    Dim hdcSource As IntPtr = s.GetHdc

                    BitBlt(hdcDest, 0, 0, obj.img.Width, obj.img.Height, hdcSource, 0, 0, &HCC0020)

                    s.ReleaseHdc()
                    g.ReleaseHdc()
                    s.Dispose()

                    'g.DrawImageUnscaled(obj.img, pt)
                End If

                g.FillRectangle(New SolidBrush(obj.fillcolor), obj.position)


            Next



            fpscalc.Stop()

            Dim newrender As Bitmap = outrender.Clone

            bck.ReportProgress(99, newrender)


            Dim mstowait As Decimal = ((1000 / FPS) - fpscalc.ElapsedMilliseconds)

            If mstowait > 0 Then
                System.Threading.Thread.Sleep(mstowait)
            Else
                g.DrawString("Can not keep up frames", New Font("Arial", 12), New SolidBrush(Color.Red), 0, 0)
            End If

            ellapsedticks += 1

        End While

        g.Dispose()

    End Sub

    Public Sub ResolveCollision(obj1 As PhysObj, obj2 As PhysObj)
        ' Calculate overlap in both axes
        Dim overlapX As Single = Math.Min(obj1.position.X + obj1.position.Width, obj2.position.X + obj2.position.Width) - Math.Max(obj1.position.X, obj2.position.X)
        Dim overlapY As Single = Math.Min(obj1.position.Y + obj1.position.Height, obj2.position.Y + obj2.position.Height) - Math.Max(obj1.position.Y, obj2.position.Y)

        ' Adjust position and velocity based on the smallest overlap
        If Math.Abs(overlapX) < Math.Abs(overlapY) Then
            ' Horizontal collision: reverse X velocities
            If obj1.position.X < obj2.position.X Then
                obj1.position.X -= overlapX / 2
                obj2.position.X += overlapX / 2
            Else
                obj1.position.X += overlapX / 2
                obj2.position.X -= overlapX / 2
            End If

            ' Apply conservation of momentum for horizontal velocities
            Dim newVelX1 As Single = ((obj1.mass - obj2.mass) * obj1.speed.X + 2 * obj2.mass * obj2.speed.X) / (obj1.mass + obj2.mass)
            Dim newVelX2 As Single = ((obj2.mass - obj1.mass) * obj2.speed.X + 2 * obj1.mass * obj1.speed.X) / (obj1.mass + obj2.mass)
            obj1.speed.X = newVelX1 * obj1.bouncyness
            obj2.speed.X = newVelX2 * obj2.bouncyness

        Else
            ' Vertical collision: reverse Y velocities
            If obj1.position.Y < obj2.position.Y Then
                obj1.position.Y -= overlapY / 2
                obj2.position.Y += overlapY / 2
            Else
                obj1.position.Y += overlapY / 2
                obj2.position.Y -= overlapY / 2
            End If

            ' Apply conservation of momentum for horizontal velocities
            Dim newVelY1 As Single = ((obj1.mass - obj2.mass) * obj1.speed.Y + 2 * obj2.mass * obj2.speed.Y) / (obj1.mass + obj2.mass)
            Dim newVelY2 As Single = ((obj2.mass - obj1.mass) * obj2.speed.Y + 2 * obj1.mass * obj1.speed.Y) / (obj1.mass + obj2.mass)
            obj1.speed.Y = newVelY1 * obj1.bouncyness
            obj2.speed.Y = newVelY2 * obj2.bouncyness
        End If
    End Sub


    Dim outrender As Bitmap
    Public Function get_CountActiveElements() As Integer
        Return collisionobjects.Count
    End Function


    Sub update_render(sender As Object, e As ProgressChangedEventArgs)

        Dim render As Bitmap = e.UserState

        If outrender IsNot Nothing Then
            outrender.Dispose()
        End If

        outrender = render.Clone

        render.Dispose()

        Dim ev As New UpdateRenderEventArgs
        ev.outrender = outrender

        RaiseEvent RenderUpdated(Me, ev)

    End Sub


    Public Function IsPointInRectangle(point As Point, rect As RectangleF) As Boolean
        Return point.X >= rect.Left AndAlso point.X <= rect.Right AndAlso
           point.Y >= rect.Top AndAlso point.Y <= rect.Bottom
    End Function



    Public Function find_physobj_at_point(position As Point) As Simulation.PhysObj

        For Each img In collisionobjects
            If IsPointInRectangle(position, img.position) Then
                Return img
            End If
        Next

        Return Nothing
    End Function


    Function add_new_obj(newobj As PhysObj, Optional ByVal respect_collission As Boolean = True) As Boolean

        If respect_collission Then

            If collideswithanyotherobj(newobj).Count = 0 AndAlso Not collideswithwall_x(newobj) AndAlso Not collideswithwall_y(newobj) Then
                objstoadd.Add(newobj)
                Return True
            End If
        Else
            objstoadd.Add(newobj)
            Return True
        End If


        Return False
    End Function




    Sub dreikörperproblem()


        Dim pos1 As New RectangleF(400, 250, 20, 20)
        Dim newobj1 As New PhysObj(pos1, New Drawing.SizeF(0, 0), 200, 100000)
        objstoadd.Add(newobj1)


        Dim pos2 As New RectangleF(400, 150, 20, 20)
        Dim newobj2 As New PhysObj(pos2, New Drawing.SizeF(4, -1), -100, 10)
        objstoadd.Add(newobj2)


        Dim pos3 As New RectangleF(400, 350, 20, 20)
        Dim newobj3 As New PhysObj(pos3, New Drawing.SizeF(-4, 1), -100, 10)
        objstoadd.Add(newobj3)

    End Sub


    Sub spawn_random_objs_christmas(num As Integer)

        Dim maxcharge As Integer = 0
        For i = 0 To num - 1
retry:
            Dim pos As New Rectangle(Rnd() * windowsize.Width, Rnd() * windowsize.Height, Rnd() * 200 + 20, Rnd() * 200 + 20)

            Dim c As CompositeObjekt = ChristmasGIF(New Point(Rnd() * windowsize.Width, Rnd() * windowsize.Height), 0.2)

            If collideswithanyotherobj(c.singles(0)).Count = 0 AndAlso collideswithwall_x(c.singles(0)) = False AndAlso collideswithwall_y(c.singles(0)) = False Then
                'c.Load()

            Else
                GoTo retry
            End If

        Next

    End Sub


    Sub spawn_random_objs(num As Integer)

        Dim maxcharge As Integer = 0
        For i = 0 To num - 1
retry:
            Dim pos As New Rectangle(Rnd() * windowsize.Width, Rnd() * windowsize.Height, Rnd() * 200 + 20, Rnd() * 200 + 20)

            Dim newobj As New PhysObj(pos, Nothing, 0, 100, True, 0.6)

            If collideswithanyotherobj(newobj).Count = 0 AndAlso collideswithwall_x(newobj) = False AndAlso collideswithwall_y(newobj) = False Then
                objstoadd.Add(newobj)
            Else
                GoTo retry
            End If

        Next
    End Sub



End Class
