using Actors;
using Data;
using Events;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Gameplay
{
    public class BuildingPlacementController : MonoBehaviour
    {
        [SerializeField] 
        private MeshFilter navMeshFilter;
        [SerializeField] 
        private GameObject placementObj;
        [SerializeField] 
        private float clickDelay;
        [SerializeField] 
        private float activeClickDelay;
    
        private BuildingActor _building;
        private int _buildingId;
        private RaycastHit _hitInfo;
        private NavMeshHit _meshHit;
        private Camera _camera;

        private Actor<BuildingActor.SpawnSettings, BuildingData>.Factory _buildingFactory;
        private SignalBus _signalBus;

        [Inject]
        public void Init(Actor<BuildingActor.SpawnSettings, BuildingData>.Factory buildingFactory,
            SignalBus signalBus)
        {
            _buildingFactory = buildingFactory;
            _signalBus = signalBus;
            _signalBus.Subscribe<InventoryBuildingPlacementSignal>(OnBuildingPlacement);
        }

        public void Awake()
        {
            navMeshFilter.gameObject.SetActive(false);
            _camera = Camera.main;
        }
    
        void Update()
        {
            activeClickDelay = Mathf.Max(0, activeClickDelay - Time.deltaTime);
            if (_building != null)
            {
                UpdatePlacement();
            }
        }
        private void OnBuildingPlacement(InventoryBuildingPlacementSignal placement)
        {
            OnStartBuildingPlacement(placement.buildingId);
        }
  
        private void OnStartBuildingPlacement(int buildingId)
        {
            if (_building == null)
            {
                AttemptStartPlacement(buildingId);
            }
            else
            {
                CancelPlacement();
            }
        }
    
        private void AttemptStartPlacement(int buildingId)
        {
            _buildingId = buildingId;
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction * 1000, out _hitInfo))
            {
                NavMeshTriangulation triangles = NavMesh.CalculateTriangulation ();
                Mesh mesh = new Mesh();
                mesh.vertices = triangles.vertices;
                mesh.triangles = triangles.indices;
                navMeshFilter.mesh = mesh;
                navMeshFilter.gameObject.SetActive(true);
                _building = _buildingFactory.Create(new BuildingActor.SpawnSettings
                {
                    dataId = buildingId,
                    position = _hitInfo.point,
                    Team = 0
                }) as BuildingActor;
                EnterBuildingPlacementMode();
            }
        }

        private void UpdatePlacement()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!placementObj.activeSelf)
            {
                var bounds = _building.ObstacleBounds();
                if (bounds != null)
                {
                    placementObj.transform.localScale = bounds.size;
                    placementObj.transform.localPosition = bounds.center;
                    placementObj.SetActive(true);
                }
            }
            _building.gameObject.SetActive(false);
            if (Physics.Raycast(ray.origin, ray.direction * 1000, out _hitInfo))
            {
                _building.transform.position = _hitInfo.point;

                if (!NavMesh.SamplePosition(_hitInfo.point, out _meshHit, 100, 1))
                {
                    if (NavMesh.FindClosestEdge(_hitInfo.point, out _meshHit, 1))
                    {
                        _building.transform.position = _meshHit.position;
                    }
                }
                else
                {
                    _building.transform.position = _meshHit.position;
                }
            }
            _building.gameObject.SetActive(true);
        
            Quaternion rot = _building.transform.rotation;
            if (Input.GetKeyDown ("q")) 
            {
                rot *= Quaternion.AngleAxis(-45, Vector3.up);
            }
         
            if (Input.GetKeyDown ("e")) 
            {
                rot *= Quaternion.AngleAxis(45, Vector3.up);
            }
            _building.transform.rotation = rot;

            if (activeClickDelay <= 0 && Input.GetMouseButtonDown(0))
            {
                FinalizePlacement();
            }
            else if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
    
        private void EnterBuildingPlacementMode()
        {
            activeClickDelay = clickDelay;
            _building.Pause();
            placementObj.transform.SetParent(_building.transform);
        }

        public void CancelPlacement()
        {
            activeClickDelay = clickDelay;
            placementObj.transform.SetParent(this.transform);
            placementObj.SetActive(false);
            _building.Despawn();
            _building = null;
            navMeshFilter.gameObject.SetActive(false);
            _signalBus.Fire<InventoryBuildingPurchaseCancelSignal>();
        }

        public void FinalizePlacement()
        {
            activeClickDelay = clickDelay;
            placementObj.transform.SetParent(this.transform);
            placementObj.SetActive(false);
            placementObj.transform.rotation = Quaternion.identity;
            _building.Resume();
            _building = null;
            navMeshFilter.gameObject.SetActive(false);
            _signalBus.Fire( signal: new InventoryBuildingPurchaseCompleteSignal
            {
                buildingId = _buildingId
            });
        }
    }
}
