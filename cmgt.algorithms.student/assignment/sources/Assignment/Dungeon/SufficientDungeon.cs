using System;
using System.ComponentModel;
using GXPEngine;
using System.Drawing;
using System.Runtime.Remoting.Messaging;

/**
 * An example of a dungeon implementation.
 * This implementation places two rooms manually but your implementation has to do it procedurally.
 */
class SufficientDungeon : Dungeon
{
    private Room room;

    public SufficientDungeon(Size pSize) : base(pSize)
    {
    }

    protected override void generate(int pMinimumRoomSize)
    {
        // Initialize the initial room covering the entire space
        room = new Room(new Rectangle(0, 0, size.Width, size.Height));
        rooms.Add(room);

        DivideRoom(room, pMinimumRoomSize);

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

        // Randomly choose a division point
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

                // Adjust the offset if the intersection is too small
                int adjustedOffsetX = Math.Min(cornerOffset, intersectsRect.Width - cornerOffset);
                int adjustedOffsetY = Math.Min(cornerOffset, intersectsRect.Height - cornerOffset);

                // Calculate the spawn area avoiding exact corners
                int spawnMinX = intersectsRect.X + adjustedOffsetX;
                int spawnMaxX = intersectsRect.X + intersectsRect.Width - adjustedOffsetX;
                int spawnMinY = intersectsRect.Y + adjustedOffsetY;
                int spawnMaxY = intersectsRect.Y + intersectsRect.Height - adjustedOffsetY;

                // Ensure the spawn range is valid
                if(spawnMinX >= spawnMaxX || spawnMinY >= spawnMaxY) continue;

                int spawnX;
                int spawnY;

                if(isVerticalWall)
                {
                    // For vertical walls, doors should be placed in the middle of the vertical intersection
                    spawnX = intersectsRect.X;
                    spawnY = Utils.Random(spawnMinY, spawnMaxY);
                } else // isHorizontalWall
                {
                    // For horizontal walls, doors should be placed in the middle of the horizontal intersection
                    spawnY = intersectsRect.Y;
                    spawnX = Utils.Random(spawnMinX, spawnMaxX);
                }

                Point spawnPoint = new Point(spawnX, spawnY);

                // Generate the door at the chosen point
                Door door = new Door(spawnPoint);
                doors.Add(door);
            }
        }

        // Ensure every room has at least one door
        foreach(Room room in rooms)
        {
            if(!HasDoor(room))
            {
                PlaceDoorInRoom(room);
            }
        }
    }

    private bool HasDoor(Room room)
    {
        foreach(Door door in doors)
        {
            if(room.area.Contains(door.location))
            {
                return true;
            }
        }
        return false;
    }

    private void PlaceDoorInRoom(Room room)
    {
        // Try to place a door on a valid wall
        foreach(Room otherRoom in rooms)
        {
            if(otherRoom == room) continue;

            Rectangle intersectsRect = Rectangle.Intersect(room.area, otherRoom.area);
            if(intersectsRect.IsEmpty) continue;

            int spawnX;
            int spawnY;
            if(intersectsRect.Width < intersectsRect.Height)
            {
                // Vertical wall
                spawnX = intersectsRect.X;
                spawnY = Utils.Random(intersectsRect.Y + 1, intersectsRect.Y + intersectsRect.Height - 1);
            } else
            {
                // Horizontal wall
                spawnX = Utils.Random(intersectsRect.X + 1, intersectsRect.X + intersectsRect.Width - 1);
                spawnY = intersectsRect.Y;
            }

            Point spawnPoint = new Point(spawnX, spawnY);
            Door door = new Door(spawnPoint);
            doors.Add(door);
            return;
        }
    }
}