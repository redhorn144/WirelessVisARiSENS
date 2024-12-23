using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.XR.ARFoundation;

public class TrackedImageInfo : MonoBehaviour
{    
    [SerializeField] CSVReader reader;
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] Colormap colormap;
    [SerializeField] GameObject transmitter;
    [SerializeField] GameObject reciever;
    [SerializeField] float speed = 10.0f;
    [SerializeField] float frameRate = 2.0f;
    [SerializeField] float startWidth = 0.05f;
    [SerializeField] float endWidth = 0.025f;
    [SerializeField] float trailTime = 0.5f;
    [SerializeField] int rnum;

    private Color startColor = Color.red;
    private Color colorChange;

    private bool firstCheck = false;
    
    private List<LineRenderer> lines;
    private List<Packet> packets;
    private float frameTime;
    private float minPower;
    private float maxPower;
    private GameObject lineRef;
    private string mapName = "viridis";

    public class Packet
    {
        public GameObject sphere;
        public List<Vector3> path;
        public float length;
        public int subdivisions;
        public List<Vector3> dividedPath;
        public int idx;

        public void MakeDividedPath()
        {
            idx = 0;
            //Debug.Log(idx);
            float divisionLength = length / (float)subdivisions;
            List<float> partials = new List<float>();
            List<Vector3> normalizedPartials = new List<Vector3>();
            dividedPath = new List<Vector3>();
            

            for (int i = 0; i < path.Count - 1; i++)
            {
                partials.Add((path[i+1] - path[i]).magnitude);
                normalizedPartials.Add((path[i+1] - path[i]).normalized);
            }
            
            dividedPath.Add(path[0]);
            float sumLength = 0;
            int pathIdx = 0;

            for (int i = 0; i < subdivisions; i++)
            {
                if (sumLength < partials[pathIdx])
                {
                    dividedPath.Add(dividedPath.Last() + divisionLength*normalizedPartials[pathIdx]);
                    sumLength += divisionLength;
                    //Debug.Log(dividedPath.Last());
                }
                else
                {
                    sumLength = 0;
                    pathIdx++;
                    i--;
                }
            }
            dividedPath.Add(path.Last());

        }
    }

    void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

    void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            //Debug.Log("new image seen");
            List<Transform> trackedTransforms = new List<Transform>();

            foreach (var trackedImage in m_TrackedImageManager.trackables)
            {
                trackedTransforms.Add(trackedImage.transform);
            }
            
            //Debug.Log(trackedTransforms[0].position);
            this.transform.position = trackedTransforms[0].position;
            this.transform.rotation = trackedTransforms[0].rotation;
            
            
            var paths = reader.pathList;
            minPower = paths[0].Path_power;
            maxPower = paths[0].Path_power;
            bool first = false;
            foreach (CSVReader.Path path in paths)
            {
                if (path.Rx_Number == rnum)
                {
                    var coords = PathProcessing(path.coordinates);

                    if (!first)
                    {
                        transmitter.SetActive(true);
                        reciever.SetActive(true);
                        transmitter.transform.localPosition = coords[0];
                        reciever.transform.localPosition = coords.Last();
                        
                        
                        
                        first = true;

                    }
                    if (path.Path_power < minPower)
                    {
                        minPower = path.Path_power;
                    }
                    if (path.Path_power > maxPower)
                    {
                        maxPower = path.Path_power;
                    }
                    //Debug.Log($"{maxPower}");
                    //Debug.Log($"{minPower}");
                    //SpawnPacket(PathProcessing(path.coordinates));
                    RenderLine(coords, path.Path_power);
                }
            }
            //AddTrails();
            StartCoroutine(PacketTrajector());
        }

        foreach (var newImage in eventArgs.updated)
        {
            var toRemove = GameObject.FindGameObjectsWithTag("pathcomponent");
            foreach (var go in toRemove)
            {
                Destroy(go);
            }
            
            //Debug.Log("new image seen");
            List<Transform> trackedTransforms = new List<Transform>();

            foreach (var trackedImage in m_TrackedImageManager.trackables)
            {
                trackedTransforms.Add(trackedImage.transform);
            }
            


            Debug.Log(trackedTransforms[0].position);
            this.transform.position = trackedTransforms[0].position;
            this.transform.rotation = trackedTransforms[0].rotation;
            

            var paths = reader.pathList;
            bool first = false;
            foreach (CSVReader.Path path in paths)
            {
                if (path.Rx_Number == rnum)
                {
                    var coords = PathProcessing(path.coordinates);

                    if (!first)
                    {
                        transmitter.SetActive(true);
                        reciever.SetActive(true);
                        transmitter.transform.localPosition = coords[0];
                        reciever.transform.localPosition = coords.Last();
                        first = true;
                        

                    }

                    if (path.Path_power < minPower)
                    {
                        minPower = path.Path_power;
                    }
                    if (path.Path_power > maxPower)
                    {
                        maxPower = path.Path_power;
                    }
                    //SpawnPacket(PathProcessing(path.coordinates));
                    RenderLine(coords, path.Path_power);
                }
            }
            //AddTrails();
            StartCoroutine(PacketTrajector());
        }
    }


    private void Start()
    {
        packets = new List<Packet>();
        frameTime = 1.0f / frameRate;
        colorChange = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    private void SpawnPacket(List<Vector3> coordList)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var scale = new Vector3(0.05f, 0.05f, 0.05f);
        sphere.transform.localScale = scale;
        sphere.transform.position = coordList[0];
       
        Packet pack = new Packet();
        pack.sphere = sphere;
        pack.path = coordList;
        pack.length = PathLength(coordList);
        pack.subdivisions = (int)Math.Floor(pack.length*frameRate/speed);
        pack.MakeDividedPath();
        packets.Add(pack);
    }

    IEnumerator PacketTrajector()
    {
        
        
        for(;;)
        {
            startColor += colorChange;
            foreach (Packet pack in packets)
            {
                Debug.Log("iterating through packets");
                if (pack.idx < pack.dividedPath.Count)
                {
                    pack.sphere.transform.position = pack.dividedPath[pack.idx];
                    pack.idx++;
                    pack.sphere.GetComponent<TrailRenderer>().startColor = startColor;
                    pack.sphere.GetComponent<TrailRenderer>().endColor = startColor;

                    

                }
            }
            yield return new WaitForSeconds(frameTime);
        }

    }

    private float PathLength(List<Vector3> coordList)
    {
        float dist = 0.0f;
        
        for (int i = 0; i < coordList.Count - 1; i++)
        {
            dist += (coordList[i + 1] - coordList[i]).magnitude;
        }

        return dist;
    }

    private void AddTrails()
    {
        foreach(Packet p in packets)
        {
            p.sphere.AddComponent<TrailRenderer>();
            TrailRenderer tr = p.sphere.GetComponent<TrailRenderer>();
            tr.material = new Material(Shader.Find("Sprites/Default"));
            tr.startColor = startColor;
            tr.endColor = startColor;
            tr.startWidth = startWidth;
            tr.endWidth = endWidth;
            tr.time = trailTime;
        }
    }

    private void RenderLine(List<Vector3> coordList, float power)
    {
        Color c = colormap.GetColor(power, minPower, maxPower, mapName);
        c.a = GetAlpha(power);
        GameObject line = new GameObject("Line");
        line.tag = "pathcomponent";
        line.transform.parent = this.transform;
        line.transform.localPosition = new Vector3(0, 0, 0);
        //line.transform.localRotation = Quaternion.Euler(0, 0, 0);
        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        //lines.Add(lineRenderer);
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.04f;

        lineRenderer.endWidth = 0.04f;
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = coordList.Count;
        lineRenderer.SetPositions(coordList.ToArray());
    }

    private float GetAlpha(float power)
    {
        return (1/((maxPower - minPower)*0.75f))*(power - minPower) + 0.75f;
    }

    private List<Vector3> PathProcessing(List<Vector3> path)
    {
        List<Vector3> outPath = new List<Vector3>();
        foreach (var vec in path)
        {
            outPath.Add(SimToUnity(vec));
        }
        
        return outPath;
    }

    private Vector3 SimToUnity(Vector3 input)
    {
        var converted = new Vector3(-1*input[0], -1*input[2], -1*input[1]);
        return converted;
    }

    private Vector3 HouseToImage(Vector3 input)
    {
        var converted = new Vector3(input[0], -1*input[2], input[1]);
        return converted;
    }

}
