using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Polygon : MonoBehaviour
{
	public GameObject[] go_raw;
	public Text[] go_text_angle;
	public Text[] go_text_distance;
	public Text perimeter_text;
	public Text area_text;
	public Text type_text;
	public Text shape_type_text; // Text for displaying shape type

	private Vector2[] go_points;
	private Text[] go_points_text_a;
	private Text[] go_points_text_d;
	private GameObject[] go_n;

	private LineRenderer lineRenderer;
	private MeshFilter filter;

	void Start()
	{
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		filter = gameObject.GetComponent<MeshFilter>();
	}

	void Update()
	{
		getAllAvailablePoints();
		draw();
		drawLines();
		calculation();
	}

	private void getAllAvailablePoints()
	{
		List<Vector2> vertices2DList = new List<Vector2>();
		List<Text> textAList = new List<Text>();
		List<Text> textDList = new List<Text>();
		List<GameObject> oList = new List<GameObject>();

		for (int i = 0; i < go_raw.Length; i++)
		{
			if (go_raw[i] != null && go_raw[i].GetComponent<MeshRenderer>().enabled)
			{
				go_text_angle[i].enabled = true;
				go_text_distance[i].enabled = true;

				vertices2DList.Add(new Vector2(go_raw[i].transform.position.x, go_raw[i].transform.position.y));
				textAList.Add(go_text_angle[i]);
				textDList.Add(go_text_distance[i]);
				oList.Add(go_raw[i]);
			}
			else
			{
				go_text_angle[i].enabled = false;
				go_text_distance[i].enabled = false;
			}
		}

		go_points_text_a = textAList.ToArray();
		go_points_text_d = textDList.ToArray();
		go_points = vertices2DList.ToArray();
		go_n = oList.ToArray();
	}

	private void draw()
	{
		Vector2[] vertices2D = go_points;
		Triangulator tr = new Triangulator(vertices2D);
		int[] indices = tr.Triangulate();

		Vector3[] vertices = new Vector3[vertices2D.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices[i] = new Vector3(go_n[i].transform.position.x, go_n[i].transform.position.y, go_n[i].transform.position.z);
		}

		Mesh msh = new Mesh();
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();

		filter.mesh = msh;
	}

	private void drawLines()
	{
		lineRenderer.positionCount = go_points.Length;

		for (int i = 0; i < go_points.Length; i++)
		{
			lineRenderer.SetPosition(i, new Vector3(go_n[i].transform.position.x, go_n[i].transform.position.y, go_n[i].transform.position.z));
		}
	}

	private void calculation()
	{
		double p = 0;
		double area = 0;
		int n = 0;

		for (int i = 0; i < go_points.Length; i++)
		{
			Vector2 v0 = (i - 1) >= 0 ? go_points[i - 1] : go_points[go_points.Length - 1];
			Vector2 v1 = go_points[i];
			Vector2 v2 = (i + 1) < go_points.Length ? go_points[i + 1] : go_points[0];

			double dv0 = distance(v0.x, v0.y, v1.x, v1.y);
			double dv1 = distance(v1.x, v1.y, v2.x, v2.y);
			double dv2 = distance(v0.x, v0.y, v2.x, v2.y);

			p += dv1;

			double temp_area = (v1.x * v2.y) - (v1.y * v2.x);
			area += temp_area;

			n++;

			double a = angle(dv0, dv1, dv2, v0.x, v0.y, v1.x, v1.y, v2.x, v2.y);
			go_points_text_a[i].text = Math.Round(a) + "°";

			Vector2 mp = midPoint(v1.x, v1.y, v2.x, v2.y);
			go_points_text_d[i].transform.parent.position = new Vector3(mp.x, mp.y, go_points_text_d[i].transform.parent.position.z);
			double az = angle_zero(v1.x, v1.y, v2.x, v2.y);
			go_points_text_d[i].transform.parent.eulerAngles = new Vector3(go_points_text_d[i].transform.parent.eulerAngles.x, go_points_text_d[i].transform.parent.eulerAngles.y, (float)az);
			go_points_text_d[i].text = Math.Round(dv1 * 2.54, 0) + "cm";
		}

		string shapeTypeText = shapeType(n);
		type_text.text = shapeTypeText;


		if (shapeTypeText.Equals("Straight"))
		{
			perimeter_text.text = "Perimeter: 0cm";
		}
		else
		{
			perimeter_text.text = "Perimeter: " + Math.Round(p * 2.54, 0) + "cm";
		}

		area_text.text = "Area: " + Math.Round(Math.Abs(area / 2) * 2.54 * 2.54, 0) + "cm²";
		shape_type_text.text = getShapeTypeText(n);
	}

	private double distance(float x1, float y1, float x2, float y2)
	{
		float a = Math.Abs(x1 - x2);
		float b = Math.Abs(y1 - y2);
		double c = Math.Sqrt(a * a + b * b);
		return c;
	}

	private double angle(double i1, double i2, double i3, float p1x, float p1y, float p2x, float p2y, float p3x, float p3y)
	{
		double k = ((i2 * i2) + (i1 * i1) - (i3 * i3)) / (2 * i1 * i2);
		double d = Math.Acos(k) * (180 / Math.PI);
		double dd = direction(p1x, p1y, p2x, p2y, p3x, p3y);
		if (dd > 0)
		{
			d = 360 - d;
		}
		return d;
	}

	private double direction(float x1, float y1, float x2, float y2, float x3, float y3)
	{
		double d = ((x2 - x1) * (y3 - y1)) - ((y2 - y1) * (x3 - x1));
		return d;
	}

	private Vector2 midPoint(float x1, float y1, float x2, float y2)
	{
		float x = (x1 + x2) / 2;
		float y = (y1 + y2) / 2;
		return new Vector2(x, y);
	}

	private double angle_zero(float x1, float y1, float x2, float y2)
	{
		double xDiff = x2 - x1;
		double yDiff = y2 - y1;
		double d = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
		return d;
	}

	private string shapeType(int points)
	{
		string r = "None";
		switch (points)
		{
		case 1:
			r = "Dot";
			break;
		case 2:
			r = "Straight";
			break;
		case 3:
			r = "Triangle";
			break;
		case 4:
			r = "Quadrangle";
			break;
		case 5:
			r = "Pentagon";
			break;
		}
		return r;
	}

	private string getShapeTypeText(int points)
	{
		string shapeTypeText = shapeType(points);

		if (shapeTypeText.Equals("Triangle"))
		{
			return "The sum of interior angles for TRIANGLE is 180°";
		}
		else if (shapeTypeText.Equals("Quadrangle"))
		{
			return "The sum of interior angles for QUADRANGLE is 360°";
		}
		else if (shapeTypeText.Equals("Pentagon"))
		{
			return "The sum of interior angles for PENTAON is 540°";
		}
		else
		{
			return "-"; 
		}
	}
}
