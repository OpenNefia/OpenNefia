﻿#region --- License ---

/*
Copyright (c) 2006 - 2008 The Open Toolkit library.
Copyright 2013 Xamarin Inc

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

#endregion --- License ---

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenNefia.Core.Maths
{
    /// <summary>
    /// Represents a 4x4 Matrix
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4 : IEquatable<Matrix4>
    {
        #region Fields

        /// <summary>
        /// Top row of the matrix
        /// </summary>
        public Vector4 Row0;

        /// <summary>
        /// 2nd row of the matrix
        /// </summary>
        public Vector4 Row1;

        /// <summary>
        /// 3rd row of the matrix
        /// </summary>
        public Vector4 Row2;

        /// <summary>
        /// Bottom row of the matrix
        /// </summary>
        public Vector4 Row3;

        /// <summary>
        /// The identity matrix
        /// </summary>
        public static readonly Matrix4 Identity = new(Vector4.UnitX, Vector4.UnitY, Vector4.UnitZ, Vector4.UnitW);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="row0">Top row of the matrix</param>
        /// <param name="row1">Second row of the matrix</param>
        /// <param name="row2">Third row of the matrix</param>
        /// <param name="row3">Bottom row of the matrix</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4(Vector4 row0, Vector4 row1, Vector4 row2, Vector4 row3)
        {
            Row0 = row0;
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="m00">First item of the first row of the matrix.</param>
        /// <param name="m01">Second item of the first row of the matrix.</param>
        /// <param name="m02">Third item of the first row of the matrix.</param>
        /// <param name="m03">Fourth item of the first row of the matrix.</param>
        /// <param name="m10">First item of the second row of the matrix.</param>
        /// <param name="m11">Second item of the second row of the matrix.</param>
        /// <param name="m12">Third item of the second row of the matrix.</param>
        /// <param name="m13">Fourth item of the second row of the matrix.</param>
        /// <param name="m20">First item of the third row of the matrix.</param>
        /// <param name="m21">Second item of the third row of the matrix.</param>
        /// <param name="m22">Third item of the third row of the matrix.</param>
        /// <param name="m23">First item of the third row of the matrix.</param>
        /// <param name="m30">Fourth item of the fourth row of the matrix.</param>
        /// <param name="m31">Second item of the fourth row of the matrix.</param>
        /// <param name="m32">Third item of the fourth row of the matrix.</param>
        /// <param name="m33">Fourth item of the fourth row of the matrix.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4(
            float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            Row0 = new Vector4(m00, m01, m02, m03);
            Row1 = new Vector4(m10, m11, m12, m13);
            Row2 = new Vector4(m20, m21, m22, m23);
            Row3 = new Vector4(m30, m31, m32, m33);
        }

        #endregion Constructors

        #region Public Members

        #region Properties

        /// <summary>
        /// The determinant of this matrix
        /// </summary>
        public float Determinant
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row0.X * Row1.Y * Row2.Z * Row3.W - Row0.X * Row1.Y * Row2.W * Row3.Z + Row0.X * Row1.Z * Row2.W * Row3.Y - Row0.X * Row1.Z * Row2.Y * Row3.W
                + Row0.X * Row1.W * Row2.Y * Row3.Z - Row0.X * Row1.W * Row2.Z * Row3.Y - Row0.Y * Row1.Z * Row2.W * Row3.X + Row0.Y * Row1.Z * Row2.X * Row3.W
                - Row0.Y * Row1.W * Row2.X * Row3.Z + Row0.Y * Row1.W * Row2.Z * Row3.X - Row0.Y * Row1.X * Row2.Z * Row3.W + Row0.Y * Row1.X * Row2.W * Row3.Z
                + Row0.Z * Row1.W * Row2.X * Row3.Y - Row0.Z * Row1.W * Row2.Y * Row3.X + Row0.Z * Row1.X * Row2.Y * Row3.W - Row0.Z * Row1.X * Row2.W * Row3.Y
                + Row0.Z * Row1.Y * Row2.W * Row3.X - Row0.Z * Row1.Y * Row2.X * Row3.W - Row0.W * Row1.X * Row2.Y * Row3.Z + Row0.W * Row1.X * Row2.Z * Row3.Y
                - Row0.W * Row1.Y * Row2.Z * Row3.X + Row0.W * Row1.Y * Row2.X * Row3.Z - Row0.W * Row1.Z * Row2.X * Row3.Y + Row0.W * Row1.Z * Row2.Y * Row3.X;
        }

        /// <summary>
        /// The first column of this matrix
        /// </summary>
        public Vector4 Column0
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Row0.X, Row1.X, Row2.X, Row3.X);
        }

        /// <summary>
        /// The second column of this matrix
        /// </summary>
        public Vector4 Column1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Row0.Y, Row1.Y, Row2.Y, Row3.Y);
        }

        /// <summary>
        /// The third column of this matrix
        /// </summary>
        public Vector4 Column2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Row0.Z, Row1.Z, Row2.Z, Row3.Z);
        }

        /// <summary>
        /// The fourth column of this matrix
        /// </summary>
        public Vector4 Column3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Row0.W, Row1.W, Row2.W, Row3.W);
        }

        /// <summary>
        /// Gets or sets the value at row 1, column 1 of this instance.
        /// </summary>
        public float M11
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row0.X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row0.X = value;
        }

        /// <summary>
        /// Gets or sets the value at row 1, column 2 of this instance.
        /// </summary>
        public float M12
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row0.Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row0.Y = value;
        }

        /// <summary>
        /// Gets or sets the value at row 1, column 3 of this instance.
        /// </summary>
        public float M13
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row0.Z;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row0.Z = value;
        }

        /// <summary>
        /// Gets or sets the value at row 1, column 4 of this instance.
        /// </summary>
        public float M14
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row0.W;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row0.W = value;
        }

        /// <summary>
        /// Gets or sets the value at row 2, column 1 of this instance.
        /// </summary>
        public float M21
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row1.X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row1.X = value;
        }

        /// <summary>
        /// Gets or sets the value at row 2, column 2 of this instance.
        /// </summary>
        public float M22
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row1.Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row1.Y = value;
        }

        /// <summary>
        /// Gets or sets the value at row 2, column 3 of this instance.
        /// </summary>
        public float M23
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row1.Z;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row1.Z = value;
        }

        /// <summary>
        /// Gets or sets the value at row 2, column 4 of this instance.
        /// </summary>
        public float M24
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row1.W;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row1.W = value;
        }

        /// <summary>
        /// Gets or sets the value at row 3, column 1 of this instance.
        /// </summary>
        public float M31
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row2.X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row2.X = value;
        }

        /// <summary>
        /// Gets or sets the value at row 3, column 2 of this instance.
        /// </summary>
        public float M32
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row2.Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row2.Y = value;
        }

        /// <summary>
        /// Gets or sets the value at row 3, column 3 of this instance.
        /// </summary>
        public float M33
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row2.Z;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row2.Z = value;
        }

        /// <summary>
        /// Gets or sets the value at row 3, column 4 of this instance.
        /// </summary>
        public float M34
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row2.W;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row2.W = value;
        }

        /// <summary>
        /// Gets or sets the value at row 4, column 1 of this instance.
        /// </summary>
        public float M41
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row3.X;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row3.X = value;
        }

        /// <summary>
        /// Gets or sets the value at row 4, column 2 of this instance.
        /// </summary>
        public float M42
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row3.Y;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row3.Y = value;
        }

        /// <summary>
        /// Gets or sets the value at row 4, column 3 of this instance.
        /// </summary>
        public float M43
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row3.Z;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row3.Z = value;
        }

        /// <summary>
        /// Gets or sets the value at row 4, column 4 of this instance.
        /// </summary>
        public float M44
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Row3.W;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Row3.W = value;
        }

        #endregion Properties

        #region Instance

        #region public void Invert()

        /// <summary>
        /// Converts this instance into its inverse.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// </summary>
        public void Invert()
        {
            Invert(ref this);
        }

        #endregion public void Invert()

        #region public void Transpose()

        /// <summary>
        /// Converts this instance into its transpose.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Transpose()
        {
            this = Transpose(this);
        }

        #endregion public void Transpose()

        #endregion Instance

        #region Static

        #region CreateFromAxisAngle

        /// <summary>
        /// Build a rotation matrix from the specified axis/angle rotation.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
        /// <param name="result">A matrix instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateFromAxisAngle(Vector3 axis, float angle, out Matrix4 result)
        {
            var cos = (float) Math.Cos(-angle);
            var sin = (float) Math.Sin(-angle);
            var t = 1.0f - cos;

            axis.Normalize();

            result = new Matrix4(t * axis.X * axis.X + cos, t * axis.X * axis.Y - sin * axis.Z, t * axis.X * axis.Z + sin * axis.Y, 0.0f,
                t * axis.X * axis.Y + sin * axis.Z, t * axis.Y * axis.Y + cos, t * axis.Y * axis.Z - sin * axis.X, 0.0f,
                t * axis.X * axis.Z - sin * axis.Y, t * axis.Y * axis.Z + sin * axis.X, t * axis.Z * axis.Z + cos, 0.0f,
                0, 0, 0, 1);
        }

        /// <summary>
        /// Build a rotation matrix from the specified axis/angle rotation.
        /// </summary>
        /// <param name="axis">The axis to rotate about.</param>
        /// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
        /// <returns>A matrix instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateFromAxisAngle(Vector3 axis, float angle)
        {
            CreateFromAxisAngle(axis, angle, out var result);
            return result;
        }

        #endregion CreateFromAxisAngle

        #region CreateRotation[XYZ]

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateRotationX(float angle, out Matrix4 result)
        {
            var cos = (float) Math.Cos(angle);
            var sin = (float) Math.Sin(angle);

            result.Row0 = Vector4.UnitX;
            result.Row1 = new Vector4(0.0f, cos, sin, 0.0f);
            result.Row2 = new Vector4(0.0f, -sin, cos, 0.0f);
            result.Row3 = Vector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the x-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateRotationX(float angle)
        {
            CreateRotationX(angle, out var result);
            return result;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateRotationY(float angle, out Matrix4 result)
        {
            var cos = (float) Math.Cos(angle);
            var sin = (float) Math.Sin(angle);

            result.Row0 = new Vector4(cos, 0.0f, -sin, 0.0f);
            result.Row1 = Vector4.UnitY;
            result.Row2 = new Vector4(sin, 0.0f, cos, 0.0f);
            result.Row3 = Vector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the y-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateRotationY(float angle)
        {
            CreateRotationY(angle, out var result);
            return result;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateRotationZ(float angle, out Matrix4 result)
        {
            var cos = (float) Math.Cos(angle);
            var sin = (float) Math.Sin(angle);

            result.Row0 = new Vector4(cos, sin, 0.0f, 0.0f);
            result.Row1 = new Vector4(-sin, cos, 0.0f, 0.0f);
            result.Row2 = Vector4.UnitZ;
            result.Row3 = Vector4.UnitW;
        }

        /// <summary>
        /// Builds a rotation matrix for a rotation around the z-axis.
        /// </summary>
        /// <param name="angle">The counter-clockwise angle in radians.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateRotationZ(float angle)
        {
            CreateRotationZ(angle, out var result);
            return result;
        }

        #endregion CreateRotation[XYZ]

        #region CreateTranslation

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="x">X translation.</param>
        /// <param name="y">Y translation.</param>
        /// <param name="z">Z translation.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateTranslation(float x, float y, float z, out Matrix4 result)
        {
            result = Identity;
            result.Row3 = new Vector4(x, y, z, 1);
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateTranslation(ref Vector3 vector, out Matrix4 result)
        {
            result = Identity;
            result.Row3 = new Vector4(vector.X, vector.Y, vector.Z, 1);
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="x">X translation.</param>
        /// <param name="y">Y translation.</param>
        /// <param name="z">Z translation.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTranslation(float x, float y, float z)
        {
            CreateTranslation(x, y, z, out var result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="vector">The translation vector.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateTranslation(Vector3 vector)
        {
            CreateTranslation(vector.X, vector.Y, vector.Z, out var result);
            return result;
        }

        #endregion CreateTranslation

        #region CreateOrthographic

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateOrthographic(float width, float height, float zNear, float zFar, out Matrix4 result)
        {
            CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, zNear, zFar, out result);
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <rereturns>The resulting Matrix4 instance.</rereturns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateOrthographic(float width, float height, float zNear, float zFar)
        {
            CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, zNear, zFar, out var result);
            return result;
        }

        #endregion CreateOrthographic

        #region CreateOrthographicOffCenter

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4 result)
        {
            result = new Matrix4();

            var invRL = 1 / (right - left);
            var invTB = 1 / (top - bottom);
            var invFN = 1 / (zFar - zNear);

            result.M11 = 2 * invRL;
            result.M22 = 2 * invTB;
            result.M33 = -2 * invFN;

            result.M41 = -(right + left) * invRL;
            result.M42 = -(top + bottom) * invTB;
            result.M43 = -(zFar + zNear) * invFN;
            result.M44 = 1;
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <returns>The resulting Matrix4 instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar, out var result);
            return result;
        }

        #endregion CreateOrthographicOffCenter

        #region CreatePerspectiveFieldOfView

        /// <summary>
        /// Creates a perspective projection matrix.
        /// </summary>
        /// <param name="fovy">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspect">Aspect ratio of the view (width / height)</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <param name="result">A projection matrix that transforms camera space to raster space</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>fovy is zero, less than zero or larger than Math.PI</item>
        /// <item>aspect is negative or zero</item>
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar, out Matrix4 result)
        {
            if (fovy <= 0 || fovy > Math.PI)
                throw new ArgumentOutOfRangeException("fovy");
            if (aspect <= 0)
                throw new ArgumentOutOfRangeException("aspect");
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");

            var yMax = zNear * (float) Math.Tan(0.5f * fovy);
            var yMin = -yMax;
            var xMin = yMin * aspect;
            var xMax = yMax * aspect;

            CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar, out result);
        }

        /// <summary>
        /// Creates a perspective projection matrix.
        /// </summary>
        /// <param name="fovy">Angle of the field of view in the y direction (in radians)</param>
        /// <param name="aspect">Aspect ratio of the view (width / height)</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>fovy is zero, less than zero or larger than Math.PI</item>
        /// <item>aspect is negative or zero</item>
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreatePerspectiveFieldOfView(float fovy, float aspect, float zNear, float zFar)
        {
            CreatePerspectiveFieldOfView(fovy, aspect, zNear, zFar, out var result);
            return result;
        }

        #endregion CreatePerspectiveFieldOfView

        #region CreatePerspectiveOffCenter

        /// <summary>
        /// Creates an perspective projection matrix.
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <param name="result">A projection matrix that transforms camera space to raster space</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4 result)
        {
            if (zNear <= 0)
                throw new ArgumentOutOfRangeException("zNear");
            if (zFar <= 0)
                throw new ArgumentOutOfRangeException("zFar");
            if (zNear >= zFar)
                throw new ArgumentOutOfRangeException("zNear");

            var x = 2.0f * zNear / (right - left);
            var y = 2.0f * zNear / (top - bottom);
            var a = (right + left) / (right - left);
            var b = (top + bottom) / (top - bottom);
            var c = -(zFar + zNear) / (zFar - zNear);
            var d = -(2.0f * zFar * zNear) / (zFar - zNear);

            result = new Matrix4(x, 0, 0, 0,
                0, y, 0, 0,
                a, b, c, -1,
                0, 0, d, 0);
        }

        /// <summary>
        /// Creates an perspective projection matrix.
        /// </summary>
        /// <param name="left">Left edge of the view frustum</param>
        /// <param name="right">Right edge of the view frustum</param>
        /// <param name="bottom">Bottom edge of the view frustum</param>
        /// <param name="top">Top edge of the view frustum</param>
        /// <param name="zNear">Distance to the near clip plane</param>
        /// <param name="zFar">Distance to the far clip plane</param>
        /// <returns>A projection matrix that transforms camera space to raster space</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown under the following conditions:
        /// <list type="bullet">
        /// <item>zNear is negative or zero</item>
        /// <item>zFar is negative or zero</item>
        /// <item>zNear is larger than zFar</item>
        /// </list>
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            CreatePerspectiveOffCenter(left, right, bottom, top, zNear, zFar, out var result);
            return result;
        }

        #endregion CreatePerspectiveOffCenter

        #region Scale Functions

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="scale">Single scale factor for x,y and z axes</param>
        /// <returns>A scaling matrix</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Scale(float scale)
        {
            return Scale(scale, scale, scale);
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="scale">Scale factors for x,y and z axes</param>
        /// <returns>A scaling matrix</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Scale(Vector3 scale)
        {
            return Scale(scale.X, scale.Y, scale.Z);
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="x">Scale factor for x-axis</param>
        /// <param name="y">Scale factor for y-axis</param>
        /// <param name="z">Scale factor for z-axis</param>
        /// <returns>A scaling matrix</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Scale(float x, float y, float z)
        {
            Matrix4 result;
            result.Row0 = Vector4.UnitX * x;
            result.Row1 = Vector4.UnitY * y;
            result.Row2 = Vector4.UnitZ * z;
            result.Row3 = Vector4.UnitW;
            return result;
        }

        #endregion Scale Functions

        #region Rotation Functions

        /// <summary>
        /// Build a rotation matrix from a quaternion
        /// </summary>
        /// <param name="q">the quaternion</param>
        /// <returns>A rotation matrix</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Rotate(Quaternion q)
        {
            q.ToAxisAngle(out var axis, out var angle);
            return CreateFromAxisAngle(axis, angle);
        }

        #endregion Rotation Functions

        #region Camera Helper Functions

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eye">Eye (camera) position in world space</param>
        /// <param name="target">Target position in world space</param>
        /// <param name="up">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A Matrix4 that transforms world space to camera space</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 LookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            var z = Vector3.Normalize(eye - target);
            var x = Vector3.Normalize(Vector3.Cross(up, z));
            var y = Vector3.Normalize(Vector3.Cross(z, x));

            var rot = new Matrix4(new Vector4(x.X, y.X, z.X, 0.0f),
                new Vector4(x.Y, y.Y, z.Y, 0.0f),
                new Vector4(x.Z, y.Z, z.Z, 0.0f),
                Vector4.UnitW);

            var trans = CreateTranslation(-eye);

            return trans * rot;
        }

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eyeX">Eye (camera) position in world space</param>
        /// <param name="eyeY">Eye (camera) position in world space</param>
        /// <param name="eyeZ">Eye (camera) position in world space</param>
        /// <param name="targetX">Target position in world space</param>
        /// <param name="targetY">Target position in world space</param>
        /// <param name="targetZ">Target position in world space</param>
        /// <param name="upX">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="upY">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <param name="upZ">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A Matrix4 that transforms world space to camera space</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 LookAt(float eyeX, float eyeY, float eyeZ, float targetX, float targetY, float targetZ, float upX, float upY, float upZ)
        {
            return LookAt(new Vector3(eyeX, eyeY, eyeZ), new Vector3(targetX, targetY, targetZ), new Vector3(upX, upY, upZ));
        }

        #endregion Camera Helper Functions

        #region Multiply Functions

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <returns>A new instance that is the result of the multiplication</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Mult(Matrix4 left, Matrix4 right)
        {
            Mult(ref left, ref right, out var result);
            return result;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The left operand of the multiplication.</param>
        /// <param name="right">The right operand of the multiplication.</param>
        /// <param name="result">A new instance that is the result of the multiplication</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Mult(ref Matrix4 left, ref Matrix4 right, out Matrix4 result)
        {
            float lM11 = left.Row0.X,
                lM12 = left.Row0.Y,
                lM13 = left.Row0.Z,
                lM14 = left.Row0.W,
                lM21 = left.Row1.X,
                lM22 = left.Row1.Y,
                lM23 = left.Row1.Z,
                lM24 = left.Row1.W,
                lM31 = left.Row2.X,
                lM32 = left.Row2.Y,
                lM33 = left.Row2.Z,
                lM34 = left.Row2.W,
                lM41 = left.Row3.X,
                lM42 = left.Row3.Y,
                lM43 = left.Row3.Z,
                lM44 = left.Row3.W,
                rM11 = right.Row0.X,
                rM12 = right.Row0.Y,
                rM13 = right.Row0.Z,
                rM14 = right.Row0.W,
                rM21 = right.Row1.X,
                rM22 = right.Row1.Y,
                rM23 = right.Row1.Z,
                rM24 = right.Row1.W,
                rM31 = right.Row2.X,
                rM32 = right.Row2.Y,
                rM33 = right.Row2.Z,
                rM34 = right.Row2.W,
                rM41 = right.Row3.X,
                rM42 = right.Row3.Y,
                rM43 = right.Row3.Z,
                rM44 = right.Row3.W;

            result.Row0.X = lM11 * rM11 + lM12 * rM21 + lM13 * rM31 + lM14 * rM41;
            result.Row0.Y = lM11 * rM12 + lM12 * rM22 + lM13 * rM32 + lM14 * rM42;
            result.Row0.Z = lM11 * rM13 + lM12 * rM23 + lM13 * rM33 + lM14 * rM43;
            result.Row0.W = lM11 * rM14 + lM12 * rM24 + lM13 * rM34 + lM14 * rM44;
            result.Row1.X = lM21 * rM11 + lM22 * rM21 + lM23 * rM31 + lM24 * rM41;
            result.Row1.Y = lM21 * rM12 + lM22 * rM22 + lM23 * rM32 + lM24 * rM42;
            result.Row1.Z = lM21 * rM13 + lM22 * rM23 + lM23 * rM33 + lM24 * rM43;
            result.Row1.W = lM21 * rM14 + lM22 * rM24 + lM23 * rM34 + lM24 * rM44;
            result.Row2.X = lM31 * rM11 + lM32 * rM21 + lM33 * rM31 + lM34 * rM41;
            result.Row2.Y = lM31 * rM12 + lM32 * rM22 + lM33 * rM32 + lM34 * rM42;
            result.Row2.Z = lM31 * rM13 + lM32 * rM23 + lM33 * rM33 + lM34 * rM43;
            result.Row2.W = lM31 * rM14 + lM32 * rM24 + lM33 * rM34 + lM34 * rM44;
            result.Row3.X = lM41 * rM11 + lM42 * rM21 + lM43 * rM31 + lM44 * rM41;
            result.Row3.Y = lM41 * rM12 + lM42 * rM22 + lM43 * rM32 + lM44 * rM42;
            result.Row3.Z = lM41 * rM13 + lM42 * rM23 + lM43 * rM33 + lM44 * rM43;
            result.Row3.W = lM41 * rM14 + lM42 * rM24 + lM43 * rM34 + lM44 * rM44;
        }

        #endregion Multiply Functions

        #region Invert Functions

        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <returns>The inverse of the given matrix if it has one, or the input if it is singular</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Matrix4 is singular.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Invert(Matrix4 mat)
        {
            var result = new Matrix4();
            mat.Invert(ref result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invert(ref Matrix4 result)
        {
            float m41 = Row3.X, m42 = Row3.Y, m43 = Row3.Z, m44 = Row3.W;
            if (m41 == 0 && m42 == 0 && m43 == 0 && m44 == 1.0f)
            {
                InvertAffine(ref result);
                return;
            }

            var d = Determinant;
            if (d == 0.0f)
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            var d1 = 1 / d;
            float m11 = Row0.X,
                m12 = Row0.Y,
                m13 = Row0.Z,
                m14 = Row0.W,
                m21 = Row1.X,
                m22 = Row1.Y,
                m23 = Row1.Z,
                m24 = Row1.W,
                m31 = Row2.X,
                m32 = Row2.Y,
                m33 = Row2.Z,
                m34 = Row2.W;

            result.Row0.X = d1 * (m22 * m33 * m44 + m23 * m34 * m42 + m24 * m32 * m43 - m22 * m34 * m43 - m23 * m32 * m44 - m24 * m33 * m42);
            result.Row0.Y = d1 * (m12 * m34 * m43 + m13 * m32 * m44 + m14 * m33 * m42 - m12 * m33 * m44 - m13 * m34 * m42 - m14 * m32 * m43);
            result.Row0.Z = d1 * (m12 * m23 * m44 + m13 * m24 * m42 + m14 * m22 * m43 - m12 * m24 * m43 - m13 * m22 * m44 - m14 * m23 * m42);
            result.Row0.W = d1 * (m12 * m24 * m33 + m13 * m22 * m34 + m14 * m23 * m32 - m12 * m23 * m34 - m13 * m24 * m32 - m14 * m22 * m33);
            result.Row1.X = d1 * (m21 * m34 * m43 + m23 * m31 * m44 + m24 * m33 * m41 - m21 * m33 * m44 - m23 * m34 * m41 - m24 * m31 * m43);
            result.Row1.Y = d1 * (m11 * m33 * m44 + m13 * m34 * m41 + m14 * m31 * m43 - m11 * m34 * m43 - m13 * m31 * m44 - m14 * m33 * m41);
            result.Row1.Z = d1 * (m11 * m24 * m43 + m13 * m21 * m44 + m14 * m23 * m41 - m11 * m23 * m44 - m13 * m24 * m41 - m14 * m21 * m43);
            result.Row1.W = d1 * (m11 * m23 * m34 + m13 * m24 * m31 + m14 * m21 * m33 - m11 * m24 * m33 - m13 * m21 * m34 - m14 * m23 * m31);
            result.Row2.X = d1 * (m21 * m32 * m44 + m22 * m34 * m41 + m24 * m31 * m42 - m21 * m34 * m42 - m22 * m31 * m44 - m24 * m32 * m41);
            result.Row2.Y = d1 * (m11 * m34 * m42 + m12 * m31 * m44 + m14 * m32 * m41 - m11 * m32 * m44 - m12 * m34 * m41 - m14 * m31 * m42);
            result.Row2.Z = d1 * (m11 * m22 * m44 + m12 * m24 * m41 + m14 * m21 * m42 - m11 * m24 * m42 - m12 * m21 * m44 - m14 * m22 * m41);
            result.Row2.W = d1 * (m11 * m24 * m32 + m12 * m21 * m34 + m14 * m22 * m31 - m11 * m22 * m34 - m12 * m24 * m31 - m14 * m21 * m32);
            result.Row3.X = d1 * (m21 * m33 * m42 + m22 * m31 * m43 + m23 * m32 * m41 - m21 * m32 * m43 - m22 * m33 * m41 - m23 * m31 * m42);
            result.Row3.Y = d1 * (m11 * m32 * m43 + m12 * m33 * m41 + m13 * m31 * m42 - m11 * m33 * m42 - m12 * m31 * m43 - m13 * m32 * m41);
            result.Row3.Z = d1 * (m11 * m23 * m42 + m12 * m21 * m43 + m13 * m22 * m41 - m11 * m22 * m43 - m12 * m23 * m41 - m13 * m21 * m42);
            result.Row3.W = d1 * (m11 * m22 * m33 + m12 * m23 * m31 + m13 * m21 * m32 - m11 * m23 * m32 - m12 * m21 * m33 - m13 * m22 * m31);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvertAffine(ref Matrix4 result)
        {
            float m11 = Row0.X,
                m12 = Row0.Y,
                m13 = Row0.Z,
                m14 = Row0.W,
                m21 = Row1.X,
                m22 = Row1.Y,
                m23 = Row1.Z,
                m24 = Row1.W,
                m31 = Row2.X,
                m32 = Row2.Y,
                m33 = Row2.Z,
                m34 = Row2.W;

            var d = m11 * m22 * m33 + m21 * m32 * m13 + m31 * m12 * m23 -
                    m11 * m32 * m23 - m31 * m22 * m13 - m21 * m12 * m33;

            if (d == 0.0f)
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            var d1 = 1 / d;

            // sub 3x3 inv
            result.Row0.X = d1 * (m22 * m33 - m23 * m32);
            result.Row0.Y = d1 * (m13 * m32 - m12 * m33);
            result.Row0.Z = d1 * (m12 * m23 - m13 * m22);
            result.Row1.X = d1 * (m23 * m31 - m21 * m33);
            result.Row1.Y = d1 * (m11 * m33 - m13 * m31);
            result.Row1.Z = d1 * (m13 * m21 - m11 * m23);
            result.Row2.X = d1 * (m21 * m32 - m22 * m31);
            result.Row2.Y = d1 * (m12 * m31 - m11 * m32);
            result.Row2.Z = d1 * (m11 * m22 - m12 * m21);

            // - sub 3x3 inv * b
            result.Row0.W = -result.Row0.X * m14 - result.Row0.Y * m24 - result.Row0.Z * m34;
            result.Row1.W = -result.Row1.X * m14 - result.Row1.Y * m24 - result.Row1.Z * m34;
            result.Row2.W = -result.Row2.X * m14 - result.Row2.Y * m24 - result.Row2.Z * m34;

            // last row remains 0 0 0 1
            result.Row3.X = result.Row3.Y = result.Row3.Z = 0.0f;
            result.Row3.W = 1.0f;
        }

        #endregion Invert Functions

        #region Transpose

        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <returns>The transpose of the given matrix</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Transpose(Matrix4 mat)
        {
            return new(mat.Column0, mat.Column1, mat.Column2, mat.Column3);
        }

        /// <summary>
        /// Calculate the transpose of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to transpose</param>
        /// <param name="result">The result of the calculation</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Transpose(ref Matrix4 mat, out Matrix4 result)
        {
            result.Row0 = mat.Column0;
            result.Row1 = mat.Column1;
            result.Row2 = mat.Column2;
            result.Row3 = mat.Column3;
        }

        #endregion Transpose

        #endregion Static

        #region Operators

        /// <summary>
        /// Matrix multiplication
        /// </summary>
        /// <param name="left">left-hand operand</param>
        /// <param name="right">right-hand operand</param>
        /// <returns>A new Matrix44 which holds the result of the multiplication</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 operator *(Matrix4 left, Matrix4 right)
        {
            return Mult(left, right);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Matrix4 left, Matrix4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Matrix4 left, Matrix4 right)
        {
            return !left.Equals(right);
        }

        #endregion Operators

        #region Overrides

        #region public override string ToString()

        /// <summary>
        /// Returns a System.String that represents the current Matrix44.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Row0}\n{Row1}\n{Row2}\n{Row3}";
        }

        #endregion public override string ToString()

        #region public override int GetHashCode()

        /// <summary>
        /// Returns the hashcode for this instance.
        /// </summary>
        /// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Row0.GetHashCode() ^ Row1.GetHashCode() ^ Row2.GetHashCode() ^ Row3.GetHashCode();
        }

        #endregion public override int GetHashCode()

        #region public override bool Equals(object obj)

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with this matrix.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool Equals(object? obj)
        {
            if (!(obj is Matrix4))
                return false;

            return Equals((Matrix4) obj);
        }

        #endregion public override bool Equals(object obj)

        #endregion Overrides

        #endregion Public Members

        #region IEquatable<Matrix4> Members

        /// <summary>Indicates whether the current matrix is equal to another matrix.</summary>
        /// <param name="other">An matrix to compare with this matrix.</param>
        /// <returns>true if the current matrix is equal to the matrix parameter; otherwise, false.</returns>
        public bool Equals(Matrix4 other)
        {
            return
                Row0 == other.Row0 &&
                Row1 == other.Row1 &&
                Row2 == other.Row2 &&
                Row3 == other.Row3;
        }

        #endregion IEquatable<Matrix4> Members
    }
}
