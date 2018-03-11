﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;
using Procedural.Model;
using Random = System.Random;

namespace Procedural.Model
{   
    public class CellMatrix: IEnumerable<Cell>, ICloneable
    {
        public CellMatrix Fill(Random random, int randomFillPercent)
        {
            this.ForEach(cell => cell.Value = IsEdge(cell) ? 1 : GenerateValue(random, randomFillPercent));
            return this;
        }

        public CellMatrix Smooth(int steps, int maxActiveNeighbors, int neighboursRadio)
        {
            for (var step = 0; step < steps; step++)
                this.WhereNot(IsEdge)
                    .ForEach(cell => {
                        var activeNeighbors = 
                            NeighboursOf(cell, neighboursRadio)
                            .Select(neighbour => neighbour.Value)
                            .Sum();
                        
                        if(activeNeighbors > maxActiveNeighbors)
                            cell.Value = 1;
                        else if (activeNeighbors < maxActiveNeighbors)
                            cell.Value = 0;
                    });
            return this;
        }

        public IEnumerable<Cell> NeighboursOf(Cell cell, int squareRadio)
        {
            var neighbours = new List<Cell>();
            
            for (var neighbourX = cell.Point.X - squareRadio; neighbourX <= cell.Point.X + squareRadio; neighbourX++)
            {
                for (var neighbourY = cell.Point.Y - squareRadio; neighbourY <= cell.Point.Y + squareRadio; neighbourY++)
                {
                    var neighbourPoint = new Point(neighbourX, neighbourY);

                    if(Contains(neighbourPoint) && !cell.Point.Equals(neighbourPoint))
                       neighbours.Add(new Cell(this, neighbourPoint));
                }
            }
            return neighbours;
        }

        public bool Contains(Point point)
        {
            return point.X >= 0 && point.X < Width && point.Y >= 0 && point.Y < Height;
        }

        public bool IsEdge(Cell cell)
        {
            return IsEdge(cell.Point);
        }

        public bool IsEdge(Point point)
        {
            return point.X == 0 || point.Y == 0 || point.X == Width - 1 || point.Y == Height - 1;
        }

        #region Properties

        private int[,] Positions
        {
            get
            {
                return map ?? (map = new int[width, height]);
            }
        }

        public Vector3 BottomLeft
        {
            get { return new Vector3(-Width / 2, 0, -Height / 2); }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        #endregion
        
        #region Private Methods
                
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Cell> GetEnumerator()
        {
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    yield return new Cell(this, new Point(x, y));
                }
            }
        }

        public CellMatrix Copy()
        {
            return (CellMatrix) Clone();
        }
        
        public object Clone()
        {
            var clone = new CellMatrix(width, height);
            if (map == null) return clone;

            clone.ForEach(cell => cell.Value = Value(cell.Point));
            return clone;
        }

        internal int Value(Point point)
        {
            return Positions[point.X, point.Y];
        }
        
        internal void Value(Point point, int value)
        {
            Positions[point.X, point.Y] = value;
        }

        int GenerateValue(Random random, int randomFillPercent)
        {
            return random.Next(0, 100) < randomFillPercent ? 1 : 0;
        }

        #endregion

        #region Constructors
        
        public CellMatrix(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        #endregion
        
        #region Attributes
        
        int[,] map;

        readonly int width;

        readonly int height;
        
        #endregion
    }
}