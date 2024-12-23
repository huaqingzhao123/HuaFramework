﻿using FPPhysics.BroadPhaseEntries;
using System;
using System.Collections.Generic;

namespace FPPhysics.BroadPhaseSystems.SortAndSweep
{
    public class Grid2DSortAndSweepQueryAccelerator : IQueryAccelerator
    {
        Grid2DSortAndSweep owner;
        public Grid2DSortAndSweepQueryAccelerator(Grid2DSortAndSweep owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Gets the broad phase associated with this query accelerator.
        /// </summary>
        public BroadPhase BroadPhase
        {
            get
            {
                return owner;
            }
        }

        public bool RayCast(Ray ray, IList<BroadPhaseEntry> outputIntersections)
        {
            throw new NotSupportedException("The Grid2DSortAndSweep broad phase cannot accelerate infinite ray casts.  Consider using a broad phase which supports infinite tests, using a custom solution, or using a finite ray.");
        }

        public bool RayCast(Ray ray, Fix64 maximumLength, IList<BroadPhaseEntry> outputIntersections)
        {
            if (maximumLength == Fix64.MaxValue)
                throw new NotSupportedException("The Grid2DSortAndSweep broad phase cannot accelerate infinite ray casts.  Consider specifying a maximum length or using a broad phase which supports infinite ray casts.");

            //Use 2d line rasterization.
            //Compute the exit location in the cell.
            //Test against each bounding box up until the exit value is reached.
            Fix64 length = Fix64.C0;
            Int2 cellIndex;
            Vector3 currentPosition = ray.Position;
            Grid2DSortAndSweep.ComputeCell(ref currentPosition, out cellIndex);
            while (true)
            {

                Fix64 cellWidth = Fix64.C1 / Grid2DSortAndSweep.cellSizeInverse;
                Fix64 nextT; //Distance along ray to next boundary.
                Fix64 nextTy; //Distance along ray to next boundary along y axis.
                Fix64 nextTz; //Distance along ray to next boundary along z axis.
                              //Find the next cell.
                if (ray.Direction.y > Fix64.C0)
                    nextTy = ((cellIndex.Y + 1) * cellWidth - currentPosition.y) / ray.Direction.y;
                else if (ray.Direction.y < Fix64.C0)
                    nextTy = ((cellIndex.Y) * cellWidth - currentPosition.y) / ray.Direction.y;
                else
                    nextTy = Fix64.MaxValue;
                if (ray.Direction.z > Fix64.C0)
                    nextTz = ((cellIndex.Z + 1) * cellWidth - currentPosition.z) / ray.Direction.z;
                else if (ray.Direction.z < Fix64.C0)
                    nextTz = ((cellIndex.Z) * cellWidth - currentPosition.z) / ray.Direction.z;
                else
                    nextTz = Fix64.MaxValue;

                bool yIsMinimum = nextTy < nextTz;
                nextT = yIsMinimum ? nextTy : nextTz;




                //Grab the cell that we are currently in.
                GridCell2D cell;
                if (owner.cellSet.TryGetCell(ref cellIndex, out cell))
                {
                    Fix64 endingX;
                    if (ray.Direction.x < Fix64.C0)
                        endingX = currentPosition.x;
                    else
                        endingX = currentPosition.x + ray.Direction.x * nextT;

                    //To fully accelerate this, the entries list would need to contain both min and max interval markers.
                    //Since it only contains the sorted min intervals, we can't just start at a point in the middle of the list.
                    //Consider some giant bounding box that spans the entire list. 
                    for (int i = 0; i < cell.entries.Count
                        && cell.entries.Elements[i].item.boundingBox.Min.x <= endingX; i++) //TODO: Try additional x axis pruning?
                    {
                        var item = cell.entries.Elements[i].item;
                        Fix64 t;
                        if (ray.Intersects(ref item.boundingBox, out t) && t < maximumLength && !outputIntersections.Contains(item))
                        {
                            outputIntersections.Add(item);
                        }
                    }
                }

                //Move the position forward.
                length += nextT;
                if (length > maximumLength) //Note that this catches the case in which the ray is pointing right down the middle of a row (resulting in a nextT of 10e10f).
                    break;
                Vector3 offset;
                Vector3.Multiply(ref ray.Direction, nextT, out offset);
                Vector3.Add(ref offset, ref currentPosition, out currentPosition);
                if (yIsMinimum)
                    if (ray.Direction.y < Fix64.C0)
                        cellIndex.Y -= 1;
                    else
                        cellIndex.Y += 1;
                else
                    if (ray.Direction.z < Fix64.C0)
                    cellIndex.Z -= 1;
                else
                    cellIndex.Z += 1;
            }
            return outputIntersections.Count > 0;

        }


        public void GetEntries(BoundingBox boundingShape, IList<BroadPhaseEntry> overlaps)
        {
            //Compute the min and max of the bounding box.
            //Loop through the cells and select bounding boxes which overlap the x axis.

            Int2 min, max;
            Grid2DSortAndSweep.ComputeCell(ref boundingShape.Min, out min);
            Grid2DSortAndSweep.ComputeCell(ref boundingShape.Max, out max);
            for (int i = min.Y; i <= max.Y; i++)
            {
                for (int j = min.Z; j <= max.Z; j++)
                {
                    //Grab the cell that we are currently in.
                    Int2 cellIndex;
                    cellIndex.Y = i;
                    cellIndex.Z = j;
                    GridCell2D cell;
                    if (owner.cellSet.TryGetCell(ref cellIndex, out cell))
                    {

                        //To fully accelerate this, the entries list would need to contain both min and max interval markers.
                        //Since it only contains the sorted min intervals, we can't just start at a point in the middle of the list.
                        //Consider some giant bounding box that spans the entire list. 
                        for (int k = 0; k < cell.entries.Count
                            && cell.entries.Elements[k].item.boundingBox.Min.x <= boundingShape.Max.x; k++) //TODO: Try additional x axis pruning? A bit of optimization potential due to overlap with AABB test.
                        {
                            bool intersects;
                            var item = cell.entries.Elements[k].item;
                            boundingShape.Intersects(ref item.boundingBox, out intersects);
                            if (intersects && !overlaps.Contains(item))
                            {
                                overlaps.Add(item);
                            }
                        }
                    }
                }
            }
        }

        public void GetEntries(BoundingSphere boundingShape, IList<BroadPhaseEntry> overlaps)
        {
            //Create a bounding box based on the bounding sphere.
            //Compute the min and max of the bounding box.
            //Loop through the cells and select bounding boxes which overlap the x axis.
#if !WINDOWS
            Vector3 offset = new Vector3();
#else
            Vector3 offset;
#endif
            offset.x = boundingShape.Radius;
            offset.y = offset.x;
            offset.z = offset.y;
            BoundingBox box;
            Vector3.Add(ref boundingShape.Center, ref offset, out box.Max);
            Vector3.Subtract(ref boundingShape.Center, ref offset, out box.Min);

            Int2 min, max;
            Grid2DSortAndSweep.ComputeCell(ref box.Min, out min);
            Grid2DSortAndSweep.ComputeCell(ref box.Max, out max);
            for (int i = min.Y; i <= max.Y; i++)
            {
                for (int j = min.Z; j <= max.Z; j++)
                {
                    //Grab the cell that we are currently in.
                    Int2 cellIndex;
                    cellIndex.Y = i;
                    cellIndex.Z = j;
                    GridCell2D cell;
                    if (owner.cellSet.TryGetCell(ref cellIndex, out cell))
                    {

                        //To fully accelerate this, the entries list would need to contain both min and max interval markers.
                        //Since it only contains the sorted min intervals, we can't just start at a point in the middle of the list.
                        //Consider some giant bounding box that spans the entire list. 
                        for (int k = 0; k < cell.entries.Count
                            && cell.entries.Elements[k].item.boundingBox.Min.x <= box.Max.x; k++) //TODO: Try additional x axis pruning? A bit of optimization potential due to overlap with AABB test.
                        {
                            bool intersects;
                            var item = cell.entries.Elements[k].item;
                            item.boundingBox.Intersects(ref boundingShape, out intersects);
                            if (intersects && !overlaps.Contains(item))
                            {
                                overlaps.Add(item);
                            }
                        }
                    }
                }
            }
        }

        //public void GetEntries(BoundingFrustum boundingShape, IList<BroadPhaseEntry> overlaps)
        //{
        //    throw new NotSupportedException("The Grid2DSortAndSweep broad phase cannot accelerate frustum tests.  Consider using a broad phase which supports frustum tests or using a custom solution.");
        //}



    }
}
