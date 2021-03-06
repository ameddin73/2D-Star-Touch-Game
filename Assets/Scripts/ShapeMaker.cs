﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeMaker : MonoBehaviour
{
    public float ejectionVelocity = 3;
    public GameObject canvas;
    private List<LineRender> _lines = new List<LineRender>();
    private List<Asteroid> _asteroids = new List<Asteroid>();

    public List<LineRender> Lines => _lines;

    public List<Asteroid> Asteroids => _asteroids;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            BadShape();
        }

        if (_lines.Count > 2 && ReferenceEquals(_lines[0].startAsteroid, _lines[_lines.Count - 1].endAsteroid))
        {
            // Get vertices
            List<Vector2> vertices = new List<Vector2>();
            foreach (var VARIABLE in _lines)
            {
                vertices.Add(VARIABLE.GetComponent<LineRenderer>().GetPosition(0));
            }

            vertices = vertices.Distinct().ToList();

            if (ShapeValidator.IsValid(vertices))
            {
                GoodShape();
            }
            else
            {
                BadShape();
            }
        }
    }

    private void BadShape()
    {
        foreach (var VARIABLE in _lines)
        {
            if (VARIABLE.startAsteroid.GetComponent<Asteroid>().Bad)
            {
                EndGame();
                break;
            }
        }
        EjectAsteroids(); //sets asteroids bad
        while (_lines.Count > 0)
        {
            Destroy(_lines[0]);
        }
    }

    private void GoodShape()
    {
        bool hasBadAsteroid = false;
        int linesCount = _lines.Count;
        while (_lines.Count > 0)
        {
            if (_lines[0].startAsteroid.GetComponent<Asteroid>().Bad)
            {
                hasBadAsteroid = true;
            }

            Destroy(_lines[0].startAsteroid);
            Destroy(_lines[0].endAsteroid);
            Destroy(_lines[0]);
        }

        if (hasBadAsteroid)
        {
            EndGame();
        }
        else
        {
            canvas.GetComponent<Score>().IncrementScore(linesCount);
        }
    }

    private void EndGame()
    {
        while (_asteroids.Count > 0)
        {
            Destroy(_asteroids[0]);
        }
        while (_lines.Count > 0)
        {
            Destroy(_lines[0]);
        }

        canvas.GetComponent<Score>().EndGame();
        Camera.main.GetComponent<DeployAsteroid>().NewGame();
    }

    private void EjectAsteroids()
    {
        Vector3 center = FindShapeCenter();
        foreach (var VARIABLE in _lines)
        {
            if (VARIABLE.startAsteroid != null)
            {
                VARIABLE.startAsteroid.GetComponent<Rigidbody2D>().velocity =
                    (VARIABLE.startAsteroid.transform.position - center).normalized * ejectionVelocity;
                StartCoroutine(VARIABLE.startAsteroid.GetComponent<Asteroid>().Eject());
            }

            if (VARIABLE.endAsteroid != null)
            {
                VARIABLE.endAsteroid.GetComponent<Rigidbody2D>().velocity =
                    (VARIABLE.endAsteroid.transform.position - center).normalized * ejectionVelocity;
                StartCoroutine(VARIABLE.startAsteroid.GetComponent<Asteroid>().Eject());
            }
        }
    }

    private Vector2 FindShapeCenter()
    {
        if (_lines.Count == 0)
        {
            return new Vector2(0, 0);
        }

        Vector2 max, min, position;
        max = min = position = _lines[0].GetPositions()[0];
        foreach (var VARIABLE in _lines)
        {
            position = VARIABLE.GetPositions()[0];
            max = Vector2.Max(max, position);
            min = Vector2.Min(min, position);
            position = VARIABLE.GetPositions()[1];
            max = Vector2.Max(max, position);
            min = Vector2.Min(min, position);
        }

        position = max - (max - min) / 2;
        return position;
    }

    public bool IsStartAsteroid(GameObject asteroid)
    {
        foreach (var VARIABLE in _lines)
        {
            if (ReferenceEquals(VARIABLE.startAsteroid, asteroid))
                return true;
        }

        return false;
    }

    public bool IsEndAsteroid(GameObject asteroid)
    {
        foreach (var VARIABLE in _lines)
        {
            if (ReferenceEquals(VARIABLE.endAsteroid, asteroid))
                return true;
        }

        return false;
    }

    public void Destroy(LineRender victim)
    {
        _lines.Remove(victim);
        if (victim)
            Destroy(victim.gameObject);
    }

    public void Destroy(Asteroid victim)
    {
        _asteroids.Remove(victim);
        if (victim)
            Destroy(victim.gameObject);
    }

    public void AddLine(LineRender line)
    {
        _lines.Add(line);
    }

    public void AddAsteroid(Asteroid asteroid)
    {
        _asteroids.Add(asteroid);
    }
}