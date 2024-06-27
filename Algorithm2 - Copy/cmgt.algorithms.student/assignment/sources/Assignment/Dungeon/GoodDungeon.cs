using System;
using System.Collections.Generic;
using System.ComponentModel;
using GXPEngine;
using System.Drawing;
using System.Runtime.Remoting.Messaging;

/**
 * An example of a dungeon implementation.
 * This implementation places two rooms manually but your implementation has to do it procedurally.
 */
class GoodDungeon : Dungeon
{
    private Room room;

    public GoodDungeon(Size pSize) : base(pSize)
    {
    }

    protected override void generate(int pMinimumRoomSize)
    {
        room = new Room(new Rectangle(0, 0, size.Width, size.Height));
        rooms.Add(room);

        DivideRoom(room, pMinimumRoomSize);
        RemoveMinMaxRooms();
        GenerateDoors(pMinimumRoomSize);
    }

    private void DivideRoom(Room room, int minRoomSize)
    {
        // Randomly decide whether to divide horizontally or vertically
        if(Utils.Random(0, 2) == 0)
        {
            DivideHorizontal(room, minRoomSize);
        } else
        {
            DivideVertical(room, minRoomSize);
        }
    }

    private void DivideHorizontal(Room room, int minRoomSize)
    {
        if(room.area.Width / 2 < minRoomSize)
        {
            return;
        }

        // Randomly choose a division
        int divisionX = Utils.Random(room.area.X + minRoomSize, room.area.X + room.area.Width - minRoomSize);

        // Left rectangle
        int leftX = room.area.X;
        int leftY = room.area.Y;
        int leftWidth = divisionX - room.area.X;
        int leftHeight = room.area.Height;
        Rectangle leftRect = new Rectangle(leftX, leftY, leftWidth, leftHeight);

        // Right rectangle
        int rightX = divisionX;
        int rightY = room.area.Y;
        int rightWidth = room.area.Width - leftWidth;
        int rightHeight = room.area.Height;
        Rectangle rightRect = new Rectangle(rightX - 1, rightY, rightWidth + 1, rightHeight);

        Room leftRoom = new Room(leftRect);
        Room rightRoom = new Room(rightRect);


        rooms.Add(leftRoom);
        rooms.Add(rightRoom);
        rooms.Remove(room);

        DivideRoom(leftRoom, minRoomSize);
        DivideRoom(rightRoom, minRoomSize);

    }

    private void DivideVertical(Room room, int minRoomSize)
    {
        if(room.area.Height / 2 < minRoomSize)
        {
            return;
        }

        // Randomly choose a division point
        int divisionY = Utils.Random(room.area.Y + minRoomSize, room.area.Y + room.area.Height - minRoomSize);

        // Top rectangle
        int topX = room.area.X;
        int topY = room.area.Y;
        int topWidth = room.area.Width;
        int topHeight = divisionY - room.area.Y;
        Rectangle topRect = new Rectangle(topX, topY, topWidth, topHeight);

        // Bottom rectangle
        int bottomX = room.area.X;
        int bottomY = divisionY;
        int bottomWidth = room.area.Width;
        int bottomHeight = room.area.Height - topHeight;
        Rectangle bottomRect = new Rectangle(bottomX, bottomY - 1, bottomWidth, bottomHeight + 1);

        Room topRoom = new Room(topRect);
        Room bottomRoom = new Room(bottomRect);

        rooms.Add(topRoom);
        rooms.Add(bottomRoom);
        rooms.Remove(room);

        DivideRoom(topRoom, minRoomSize);
        DivideRoom(bottomRoom, minRoomSize);

    }

    void GenerateDoors(int minRoomSize)
    {
        int cornerOffset = 2;
        int spawnX;
        int spawnY;

        for(int i = 0; i < rooms.Count - 1; ++i)
        {
            for(int j = i + 1; j < rooms.Count; ++j)
            {
                Rectangle room1 = rooms[j].area;
                Rectangle room2 = rooms[i].area;
                Rectangle intersectsRect = Rectangle.Intersect(room2, room1);

                if(intersectsRect.IsEmpty) continue;


                bool isVerticalWall = intersectsRect.Width <= minRoomSize && intersectsRect.Height >= minRoomSize;
                bool isHorizontalWall = intersectsRect.Height <= minRoomSize && intersectsRect.Width >= minRoomSize;

                if(!isVerticalWall && !isHorizontalWall) continue;

                int spawnMinX = intersectsRect.X + cornerOffset;
                int spawnMaxX = intersectsRect.X + intersectsRect.Width - cornerOffset;
                int spawnMinY = intersectsRect.Y + cornerOffset;
                int spawnMaxY = intersectsRect.Y + intersectsRect.Height - cornerOffset;

                if(isVerticalWall)
                {
                    spawnX = intersectsRect.X;
                    spawnY = Utils.Random(spawnMinY, spawnMaxY);
                } else // isHorizontalWall
                {
                    spawnY = intersectsRect.Y;
                    spawnX = Utils.Random(spawnMinX, spawnMaxX);
                }

                Point spawnPoint = new Point(spawnX, spawnY);
                Door door = new Door(spawnPoint);
                doors.Add(door);
            }
        }
    }

    private void RemoveMinMaxRooms()
    {
        int minArea = int.MaxValue;
        int maxArea = int.MinValue;

        foreach(Room room in rooms)
        {
            int area = room.area.Width * room.area.Height;
            if (area < minArea)
            {
                minArea = area;
            }

            if (area > maxArea)
            {
                maxArea = area;
            }
        }

        rooms.RemoveAll(room => room.area.Width * room.area.Height == minArea ||  room.area.Width * room.area.Height == maxArea);
    }

    public override void drawRooms(IEnumerable<Room> pRooms, Pen pWallColor, Brush pFillColor = null)
    {
        foreach(Room room in rooms)
        {
            int doorCount = CountDoorsInRoom(room);

            Color roomColor = GetRoomColorByDoorCount(doorCount);

            drawRoom(room, Pens.Black, new SolidBrush(roomColor));
        }
    }

    private int CountDoorsInRoom(Room room)
    {
        int count = 0;
        foreach(Door door in doors)
        {
            if(room.area.Contains(door.location))
            {
                count++;
            }
        }
        return count;
    }

    private Color GetRoomColorByDoorCount(int doorCount)
    {
        switch(doorCount)
        {
            case 0:
                return Color.Red;
            case 1:
                return Color.Orange;
            case 2:
                return Color.Yellow;
            case 3:
                return Color.Green;
            default:
                return Color.Purple;
        }
    }
}