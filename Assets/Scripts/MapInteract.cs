using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TigerForge;
using TMPro;
using SimpleFileBrowser;
using System.Linq;
using System.IO;

namespace SpatialNotes
{

    public class MapInteract : MonoBehaviour
    {
        private GameObject sideMenuNoSelectAddLocButton;
        private GameObject sideMenu;
        [SerializeField]
        private GameObject emptySideMenuNoSelection;
        [SerializeField]
        private GameObject sideMenuCreateLocation;
        private GameObject sideMenuShowLocation;
        private GameObject sideMenuEditLocation;
        private GameObject sideMenuShowLocationContent;
        private GameObject sideMenuCreateLocationButtonAdd;
        private GameObject sideMenuCreateLocationButtonCancel;
        private GameObject sideMenuCreateLocationNameField;
        private GameObject sideMenuCreateLocationDescriptionField;
        private GameObject sideMenuCreateLocationIsImageSelected;
        private GameObject showMorePanel;
        public GameObject pinsFolder;
        public GameObject locationPinsFolder;
        public GameObject pinPrefab;
        public Button createButton;
        public Camera cam;
        public Main main;
        public MapObject map;
        private GameObject mapCanvas;
        private GameObject mapImageZoom;
        private Vector3 savedUiPosOnClick;

        public GameObject zoomScrollBar;
        private GameObject staticCanvas;

        public float ZoomLevel = 1.0f;
        public float maxZoom = 50.0f;
        public float startingZoomPercent = 0.25f;
        private float currentZoom = 0.0f;
        public float minZoom = 0.0f;
        public float zoomSpeed = 2.0f;
        public float panSpeed = 1.3f;

        private bool _explorerActive = false;

        private GameObject _imageToCreateWith;
        private string _pathToImageBeingCreated = "";

        // Default path to open file explorer
        private string _defaultPath;
        private string[] _foundPaths;

        private enum mouseButton { Left, Right, Middle };

        //Variables needed for drag
        private Vector3 dragOrigin;
        private Vector3 worldSpaceOrigin;
        private bool dragging = false;
        private Vector3 dragChange;

        private Vector3 lastSelectedLocation;
        private Vector3 lastCandidateUiPos;

        //placeholder image for location if none
        public Sprite placeholderImage;

        public LocationInfo _selectedLocationInfo; // The location info of the selected location

        // ----------------- Variables for postcard -----------------
        public GameObject postcardScrollView;
        private GameObject _postcardScrollViewContent;
        private GameObject _postcardAddPostButton;

        public GameObject prefabTextPost;
        public GameObject prefabImagePost;
        private GameObject _postCardContentSection;

        public GameObject makepostcardMenu;
        private GameObject _makePostCartMenuCancel;
        private GameObject _makePostCartMenuSubmit;
        private GameObject _makePostCartMenuTitle;
        private GameObject _makePostCartMenuAddTextContent;
        private GameObject _makePostCartMenuAddImageContent;
        private GameObject _makePostCartMenuContentHolder;

        public TextMediaPost _currentTextMediaPost;

        public GameObject prefabPostCard;

        public GameObject prefabTextPostSubmitted;
        public GameObject prefabImagePostSubmitted;

        public enum SortMode { Date, Name, Location };
        public SortMode _currentSortMode = SortMode.Date;


        // ----------------- Methods -----------------
        void Start()
        {
            createButton.onClick.AddListener(addButtonClick);
            map = main.map;
            //map.DisplayMapInfo();

            //get canvas
            staticCanvas = GameObject.Find("StaticCanvas");
            mapCanvas = GameObject.Find("MapCanvas");
            if (mapCanvas == null)
            {
                return;
            }
            // Get the map image
            mapImageZoom = mapCanvas.transform.Find("MapImage").gameObject;

            // Get Scroll Bar for zooming
            zoomScrollBar = GameObject.Find("ScrollZoomBar").transform.GetChild(0).gameObject;
            //define initial, min, max zoom
            zoomScrollBar.GetComponent<Scrollbar>().value = startingZoomPercent;
            currentZoom = maxZoom * startingZoomPercent;

            // Get the empty side menu
            sideMenu = staticCanvas.transform.Find("SideMenuEmpty").gameObject;
            emptySideMenuNoSelection = sideMenu.transform.Find("SideMenuNoSelect").gameObject;
            sideMenuCreateLocation = sideMenu.transform.Find("SideMenuCreateLocation").gameObject;
            sideMenuShowLocation = sideMenu.transform.Find("SideMenuExistingLocation").gameObject;
            sideMenuEditLocation = sideMenu.transform.Find("SideMenuEditLocation").gameObject;
            //get content child inside viewport of scroll view of sidemenushowlocation
            sideMenuShowLocationContent = sideMenuShowLocation.transform.Find("Scroll View").transform.Find("Viewport").transform.Find("Content").gameObject;
            showMorePanel = sideMenuShowLocationContent.transform.Find("LocationImage").gameObject.transform.Find("ShowMore").gameObject.transform.Find("Panel").gameObject;

            // Get the location button
            sideMenuCreateLocationButtonAdd = sideMenuCreateLocation.transform.Find("Add").gameObject;
            sideMenuCreateLocationButtonCancel = sideMenuCreateLocation.transform.Find("Cancel").gameObject;
            sideMenuCreateLocationDescriptionField = sideMenuCreateLocation.transform.Find("Description").gameObject;
            sideMenuCreateLocationNameField = sideMenuCreateLocation.transform.Find("Name").gameObject;
            sideMenuCreateLocationIsImageSelected = sideMenuCreateLocation.transform.Find("IsImage").gameObject;
            _imageToCreateWith = sideMenuCreateLocation.transform.Find("DisplayIMG").gameObject;
            sideMenuNoSelectAddLocButton = emptySideMenuNoSelection.transform.Find("AddLocButton").gameObject;
            _postcardScrollViewContent = postcardScrollView.transform.Find("Viewport").gameObject;
            _postcardScrollViewContent = _postcardScrollViewContent.transform.Find("Content").gameObject;
            _postCardContentSection = _postcardScrollViewContent.transform.Find("PostContent").gameObject;
            _postcardAddPostButton = _postcardScrollViewContent.transform.Find("AddPostSection").gameObject;
            _postcardAddPostButton = _postcardAddPostButton.transform.Find("AddPostCard").gameObject;
            _makePostCartMenuCancel = makepostcardMenu.transform.Find("Trash").gameObject;
            _makePostCartMenuSubmit = makepostcardMenu.transform.Find("Post").gameObject;
            _makePostCartMenuTitle = makepostcardMenu.transform.Find("Subject").gameObject;
            _makePostCartMenuContentHolder = makepostcardMenu.transform.Find("Scroll View").gameObject;
            _makePostCartMenuContentHolder = _makePostCartMenuContentHolder.transform.Find("Viewport").gameObject;
            _makePostCartMenuContentHolder = _makePostCartMenuContentHolder.transform.Find("Content").gameObject;
            _makePostCartMenuAddImageContent = makepostcardMenu.transform.Find("AddImage").gameObject;
            _makePostCartMenuAddTextContent = makepostcardMenu.transform.Find("AddTextbox").gameObject;

            _imageToCreateWith.SetActive(false);

            // Add button listeners
            sideMenuCreateLocationButtonAdd.GetComponent<Button>().onClick.AddListener(LocationAdd);
            sideMenuCreateLocationButtonCancel.GetComponent<Button>().onClick.AddListener(LocationCancel);
            sideMenuNoSelectAddLocButton.GetComponent<Button>().onClick.AddListener(_existingMenuAddLocButton);
            sideMenuEditLocation.transform.Find("Submit").GetComponent<Button>().onClick.AddListener(SubmitEditLocation);
            sideMenuEditLocation.transform.Find("Cancel").GetComponent<Button>().onClick.AddListener(CancelEditLocation);
            _postcardAddPostButton.GetComponent<Button>().onClick.AddListener(OpenPostcardMenu);
            _makePostCartMenuCancel.GetComponent<Button>().onClick.AddListener(ClosepostcardMenu);
            _makePostCartMenuSubmit.GetComponent<Button>().onClick.AddListener(SubmitPostcard);
            _makePostCartMenuAddTextContent.GetComponent<Button>().onClick.AddListener(AddTextContent);
            _makePostCartMenuAddImageContent.GetComponent<Button>().onClick.AddListener(AddImageContent);

            //Add dropdown sortmode listener
            GameObject sortModeDropdown = _postcardScrollViewContent.transform.Find("AddPostSection").gameObject;
            sortModeDropdown = sortModeDropdown.transform.Find("Dropdown").gameObject;
            sortModeDropdown.GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate { ChangeSortModeDropdown(sortModeDropdown.GetComponent<TMP_Dropdown>()); });





            // Hide the side menu
            showMorePanel.SetActive(false);

            _removeAllPins(pinsFolder);
            _hideSideMenu();

            //load locations
            OnStartLoadLocations();
        }

        void Update()
        {
            bool _IsGamePaused = EventManager.GetBool("GAME_PAUSED");
            if (_IsGamePaused)
            {
                return;
            }
            bool IsInMenu = _getSideMenUIsActive();


            //Handle zoom in and out WIP
            if (!_explorerActive && !IsInMenu)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                {
                    _zoomIn();
                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                {
                    _zoomOut();
                }
            }

            // middle mouse or esc button
            if (Input.GetMouseButtonDown((int)mouseButton.Middle) || Input.GetKeyDown(KeyCode.Escape))
            {
                _removeAllPins(pinsFolder);
                _hideSideMenu();
                EventManager.SetData("CURSOR_NAME", "NORMAL");
                EventManager.EmitEvent("CURSOR_REFRESH");
            }



            //set cursor to normal if in menu and if right click is not held
            if (IsInMenu && EventManager.GetString("CURSOR_NAME") != "NORMAL" && !Input.GetMouseButton((int)mouseButton.Right))
            {
                EventManager.SetData("CURSOR_NAME", "NORMAL");
                EventManager.EmitEvent("CURSOR_REFRESH");
                return;
            }
            else if (EventManager.GetString("CURSOR_NAME") != "CIRCLE" && Input.GetMouseButton((int)mouseButton.Right))
            {
                EventManager.SetData("CURSOR_NAME", "CIRCLE");
                EventManager.EmitEvent("CURSOR_REFRESH");
            }
            //is in menu but not right click held
            if (IsInMenu)
            {
                return;
            }

            // move right click menu to mouse position
            if (Input.GetMouseButtonUp((int)mouseButton.Right))
            {

                _removeAllPins(pinsFolder);
                //Instantiate Pin
                lastCandidateUiPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                lastSelectedLocation = lastCandidateUiPos;
                Vector3 worldClickPos = _getWorldClickPosition(lastSelectedLocation);



                GameObject clickedPin = checkIfLocationPinClicked(worldClickPos);
                if (clickedPin != null)
                {
                    _selectedLocationInfo = clickedPin.GetComponent<PinID>().locationInfo;
                    _showSideMenuShowLocation(clickedPin);

                }
                else
                {
                    GameObject pin = Instantiate(pinPrefab, new Vector3(worldClickPos.x, worldClickPos.y, 1), Quaternion.identity);
                    pin.transform.SetParent(pinsFolder.transform);
                    //Populate pin name
                    GameObject pinName = pin.transform.Find("PinName").gameObject;
                    pinName.GetComponent<TextMeshProUGUI>().text = "Selected Location";

                    //populate pinID
                    PinID id = pin.GetComponent<PinID>();
                    id.locationInfo = new LocationInfo("Selected Location", "Selected Location", worldClickPos, "");
                    id.type = "Selected";

                    _HideAllSideMenuBranches();
                    _showSideMenuNoSelection();
                }
            }




            // Left Click Drag
            if (Input.GetMouseButtonDown((int)mouseButton.Left))
            {
                dragging = true;
                dragOrigin = Input.mousePosition;
                worldSpaceOrigin = cam.transform.position;
                EventManager.SetData("CURSOR_NAME", "NORMAL");
                EventManager.EmitEvent("CURSOR_REFRESH");
                return;
            }
            else if (Input.GetMouseButtonUp((int)mouseButton.Left))
            {
                dragging = false;
                return;
            }

            // Dragging
            if (dragging && !_explorerActive)
            {
                //change cursor if not already changed
                if (EventManager.GetString("CURSOR_NAME") != "DRAG")
                {
                    EventManager.SetData("CURSOR_NAME", "DRAG");
                    EventManager.EmitEvent("CURSOR_REFRESH");
                }

                Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
                float zoomAdjustedPanSpeed = Mathf.Clamp((float)(panSpeed * (1 - currentZoom / maxZoom)), 0.15f * panSpeed, 2.0f * panSpeed);
                Vector3 move = new Vector3(pos.x * 100 * zoomAdjustedPanSpeed, pos.y * 100 * zoomAdjustedPanSpeed, 0);
                if (dragChange == null || move != dragChange)
                {
                    dragChange = move;
                    cam.transform.position = worldSpaceOrigin - move;
                }
            }
            else
            {
                if (EventManager.GetString("CURSOR_NAME") == "DRAG")
                {
                    EventManager.SetData("CURSOR_NAME", "NORMAL");
                    EventManager.EmitEvent("CURSOR_REFRESH");
                }
            }
        }

        public void OnStartLoadLocations()
        {
            // Get all locations from the database
            Dictionary<string, LocationInfo> locations = map.GetLocations();

            foreach (KeyValuePair<string, LocationInfo> loc in locations)
            {
                //add pin to the map
                Vector3 coord = map._convertCoordStr2Vec3(loc.Key);
                GameObject pin = Instantiate(pinPrefab, new Vector3(coord.x, coord.y, 1), Quaternion.identity);
                pin.transform.SetParent(locationPinsFolder.transform);

                //Populate pin name
                GameObject pinName = pin.transform.Find("PinName").gameObject;
                pinName.GetComponent<TextMeshProUGUI>().text = loc.Value.locationName;

                //populate pinID
                loc.Value.coordinate = coord;
                PinID id = pin.GetComponent<PinID>();
                id.locationInfo = loc.Value;
                id.type = "Location";

                //pins of locations are green
                pin.GetComponent<Image>().color = Color.green;

            }

        }

        private void _existingMenuAddLocButton()
        {
            _showSideMenuCreateLocation();
        }

        // upload image from file explorer for location
        public void OnClickUploadImage()
        {
            _openFileExplorerToLoadImages();
        }

        // Helper to open the file explorer to pick only images
        private void _openFileExplorerToLoadImages()
        {
            if (_explorerActive)
            {
                TriggerPopup("Explorer already active", "Explorer Active", "xmark");
                return;
            }
            //open in maps directory, if it exists, else create it
            string path = Application.streamingAssetsPath + "/Maps";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            _setFileBrowserFilterImagesOnly();

            // Show a save file dialog, await response from dialog
            StartCoroutine(ShowLoadDialogCoroutineImagesOnly());

            return;
        }

        // Filter for only images
        private void _setFileBrowserFilterImagesOnly()
        {
            //only allow directories to be selected
            FileBrowser.DisplayedEntriesFilter += (entry) =>
            {
                if (entry.IsDirectory)
                    return true; // Don't filter folders


                string extension = Path.GetExtension(entry.Path).ToLower();
                if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || extension == ".bmp" || extension == ".tiff")
                {
                    return true;
                }


                return false; // Filter files
            };

        }

        // Coroutine to show the load file dialog for images
        IEnumerator ShowLoadDialogCoroutineImagesOnly()
        {
            _explorerActive = true;
            // Directories only
            yield return FileBrowser.WaitForLoadDialog(SimpleFileBrowser.FileBrowser.PickMode.Files, true, _defaultPath, null, "Select Map Folder", "Load");
            if (FileBrowser.Success)
            {
                OnFolderSelectedLoadFileImages(FileBrowser.Result);
            }
            else
            {
                _explorerActive = false;
                try
                {
                    string[] error = FileBrowser.Result;
                    string errorString = string.Join(", ", error);
                    TriggerPopup("Error: " + errorString, "Error", "xmark");
                }
                catch (System.Exception e)
                {
                    Debug.Log("Error: " + e.Message);
                }
            }
            _explorerActive = false;
        }

        private void _openFileExplorerToLoadImages_PostContent(string uuid)
        {
            if (_explorerActive)
            {
                TriggerPopup("Explorer already active", "Explorer Active", "xmark");
                return;
            }
            //open in maps directory, if it exists, else create it
            string path = Application.streamingAssetsPath + "/Maps";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            _setFileBrowserFilterImagesOnly();

            // Show a save file dialog, await response from dialog
            StartCoroutine(ShowLoadDialogCoroutineImagesOnly_PostContent(uuid));

            return;
        }

        IEnumerator ShowLoadDialogCoroutineImagesOnly_PostContent(string uuid)
        {
            _explorerActive = true;
            // Directories only
            yield return FileBrowser.WaitForLoadDialog(SimpleFileBrowser.FileBrowser.PickMode.Files, true, _defaultPath, null, "Select Image", "Load");
            if (FileBrowser.Success)
            {
                OnFolderSelectedLoadFileImages_PostContent(FileBrowser.Result, uuid);
            }
            else
            {
                _explorerActive = false;
                try
                {
                    string[] error = FileBrowser.Result;
                    string errorString = string.Join(", ", error);
                    TriggerPopup("Error: " + errorString, "Error", "xmark");
                }
                catch (System.Exception e)
                {
                    Debug.Log("Error: " + e.Message);

                }
            }
            _explorerActive = false;
        }

        // Logs the paths of the selected files
        void OnFolderSelectedLoadFileImages_PostContent(string[] paths, string uuid)
        {
            if (paths.Length == 0)
            {
                TriggerPopup("No files were selected", "No Files Selected", "xmark");
                return;
            }

            // select only one 
            string selectedPath = paths[0];

            //move image file to map directory and change path to new path
            string newImagePath = Application.streamingAssetsPath + map.mapAssetsPath + "/" + Path.GetFileName(selectedPath);
            File.Copy(selectedPath, newImagePath, true);

            selectedPath = newImagePath;

            //get postcard menu image from uuid
            GameObject postcardImage = _makePostCartMenuContentHolder.transform.Find(uuid).gameObject;

            //Try to load the image
            Texture2D texture = new Texture2D(2, 2);
            byte[] fileData;
            if (File.Exists(selectedPath))
            {
                fileData = File.ReadAllBytes(selectedPath);
                texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                postcardImage.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                TriggerPopup("File does not exist: " + selectedPath, "File Not Found", "xmark");
                _explorerActive = false;
                return;
            }

            //cut path to only be after the streaming assets path
            selectedPath = selectedPath.Replace(Application.streamingAssetsPath, "");




            // get placeholder tmp_text
            GameObject tmp_text = postcardImage.transform.Find("image_path").gameObject;
            tmp_text.GetComponent<TMP_Text>().text = selectedPath;
            //hide add image button
            GameObject addImageButton = postcardImage.transform.Find("AddImage").gameObject.transform.Find("Upload").gameObject;
            addImageButton.SetActive(false);


            // Trigger popup
            TriggerPopup(text: "Image " + Path.GetFileName(selectedPath) + " loaded", title: "Image Loaded", imageName: "checkmark");

        }

        // Logs the paths of the selected files
        void OnFolderSelectedLoadFileImages(string[] paths)
        {
            if (paths.Length == 0)
            {
                TriggerPopup("No files were selected", "No Files Selected", "xmark");
                return;
            }

            // select only one 
            string selectedPath = paths[0];

            //move image file to map directory and change path to new path
            string newImagePath = Application.streamingAssetsPath + map.mapAssetsPath + "/" + Path.GetFileName(selectedPath);
            File.Copy(selectedPath, newImagePath, true);
            _pathToImageBeingCreated = newImagePath;
            selectedPath = newImagePath;


            //Try to load the image
            Texture2D texture = new Texture2D(2, 2);
            byte[] fileData;
            if (File.Exists(selectedPath))
            {
                fileData = File.ReadAllBytes(selectedPath);
                texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                _imageToCreateWith.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                _pathToImageBeingCreated = selectedPath;
            }
            else
            {
                TriggerPopup("File does not exist: " + selectedPath, "File Not Found", "xmark");
                _explorerActive = false;
                return;
            }

            // show image and hide placeholder
            _imageToCreateWith.SetActive(true);
            sideMenuCreateLocationIsImageSelected.SetActive(false);



            // Trigger popup
            TriggerPopup(text: "Image " + Path.GetFileName(selectedPath) + " loaded", title: "Image Loaded", imageName: "checkmark");

        }


        public void LocationAdd()
        {
            Vector3 coord = _getWorldClickPosition(lastSelectedLocation);
            string locationName = sideMenuCreateLocationNameField.GetComponent<TMP_InputField>().text;
            string locationDescription = sideMenuCreateLocationDescriptionField.GetComponent<TMP_InputField>().text;

            string locationImagePath = "";
            if (_pathToImageBeingCreated != "")
            {
                locationImagePath = _pathToImageBeingCreated;
            }
            //Add location to the database
            map.AddLocation(locCoord: coord, locName: locationName, locDescription: locationDescription, locImagePath: locationImagePath);

            //add pin to the map
            GameObject pin = Instantiate(pinPrefab, new Vector3(coord.x, coord.y, 1), Quaternion.identity);
            pin.transform.SetParent(locationPinsFolder.transform);
            //Populate pin name
            GameObject pinName = pin.transform.Find("PinName").gameObject;
            pinName.GetComponent<TextMeshProUGUI>().text = locationName;

            //populate pinID
            PinID id = pin.GetComponent<PinID>();
            id.locationInfo = new LocationInfo(locationName, locationDescription, coord, locationImagePath);
            id.type = "Location";

            //pins of locations are green
            pin.GetComponent<Image>().color = Color.green;

            //call for save to file
            map.SaveAll();

            //clear the fields for the next location
            sideMenuCreateLocationNameField.GetComponent<TMP_InputField>().text = "";
            sideMenuCreateLocationDescriptionField.GetComponent<TMP_InputField>().text = "";
            _pathToImageBeingCreated = "";
            //hide the image
            _imageToCreateWith.SetActive(false);
            sideMenuCreateLocationIsImageSelected.SetActive(true);

            //remove temporary pins
            _removeAllPins(pinsFolder);


            //Close the add location canvas
            _hideSideMenu();
        }
        public void LocationCancel()
        {
            _hideSideMenu();

            //clear the fields for the next location
            sideMenuCreateLocationNameField.GetComponent<TMP_InputField>().text = "";
            sideMenuCreateLocationDescriptionField.GetComponent<TMP_InputField>().text = "";
            _pathToImageBeingCreated = "";
            _imageToCreateWith.SetActive(false);
            sideMenuCreateLocationIsImageSelected.SetActive(true);
        }

        private bool _getSideMenUIsActive()
        {
            return sideMenuCreateLocation.activeInHierarchy || sideMenuEditLocation.activeInHierarchy || makepostcardMenu.activeInHierarchy || sideMenuShowLocation.activeInHierarchy;
        }

        private void _hideSideMenu()
        {
            _HideAllSideMenuBranches();
            sideMenu.SetActive(false);
        }

        private void _HideAllSideMenuBranches()
        {
            emptySideMenuNoSelection.SetActive(false);
            sideMenuCreateLocation.SetActive(false);
            sideMenuEditLocation.SetActive(false);
            ClosepostcardMenu();
            _hideSideMenuShowLocation();
        }

        private GameObject _getClosestLocationToThreshold(Vector2 worldpos, float threshold = 10.0f)
        {
            // Get all pins
            List<GameObject> locationPins = new List<GameObject>();
            List<Vector3> locationPinPositions = new List<Vector3>();
            foreach (Transform child in locationPinsFolder.transform)
            {
                locationPins.Add(child.gameObject);
                locationPinPositions.Add(child.position);
            }

            // Get the distance to each pin
            for (int i = 0; i < locationPins.Count; i++)
            {
                float distance = Vector3.Distance(worldpos, locationPinPositions[i]);
                if (distance < threshold)
                {
                    return locationPins[i];
                }
            }
            return null;
        }

        private GameObject checkIfLocationPinClicked(Vector2 worldpos)
        {
            float distThreshold = 3.5f / ZoomLevel;
            GameObject closestPin = _getClosestLocationToThreshold(worldpos, distThreshold);
            if (closestPin != null)
            {
                return closestPin;
            }
            return null;
        }


        private void _zoomIn()
        {
            // Zoom camera in 
            float _zoomAmt = zoomSpeed;
            //don't zoom if we are at the max zoom
            if (currentZoom + _zoomAmt > maxZoom)
            {
                return;
            }
            currentZoom += _zoomAmt;

            zoomScrollBar.GetComponent<Scrollbar>().value = currentZoom / maxZoom;

            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z + _zoomAmt);


        }

        private void _zoomOut()
        {
            // Zoom camera out
            float _zoomAmt = zoomSpeed;
            //don't zoom if we are at the min zoom
            if (currentZoom - _zoomAmt < minZoom)
            {
                return;
            }
            currentZoom -= _zoomAmt;
            zoomScrollBar.GetComponent<Scrollbar>().value = currentZoom / maxZoom;

            cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, cam.transform.position.z - zoomSpeed);
        }

        private Vector3 _getWorldClickPosition(Vector3 lastCandidateUiPos)
        {
            //adjust for camera offset
            lastCandidateUiPos.z = Mathf.Abs(cam.transform.position.z);

            return cam.ScreenToWorldPoint(lastCandidateUiPos);
        }

        private Vector4 _calcVisibleImageDims()
        {
            Vector2 fullSize = new Vector2(1600, 900); // Full size of the image (before zooming)
            Vector2 visibleSize = new Vector2(Screen.width / ZoomLevel, Screen.height / ZoomLevel);
            Vector2 centerPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector2 topLeft = centerPoint - (visibleSize / 2);
            Vector2 bottomRight = centerPoint + (visibleSize / 2);
            Vector2 visibleDims = bottomRight - topLeft;
            return new Vector4(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }


        private void _showSideMenuNoSelection()
        {
            sideMenu.SetActive(true);
            _HideAllSideMenuBranches();

            Vector3 coord = _getWorldClickPosition(lastSelectedLocation);

            //Set text to the location
            TextMeshProUGUI locationText = emptySideMenuNoSelection.transform.Find("Coordinates").GetComponent<TextMeshProUGUI>();
            locationText.text = "(" + coord.x + ", " + coord.y + ")";

            emptySideMenuNoSelection.SetActive(true);

        }

        private void _showSideMenuEmpty()
        {
            sideMenu.SetActive(true);
            _HideAllSideMenuBranches();
        }

        private void _showSideMenuCreateLocation()
        {
            _showSideMenuEmpty();
            sideMenuCreateLocation.SetActive(true);
        }

        private void _showSideMenuShowLocation(GameObject locationPin)
        {
            _showSideMenuEmpty();
            _drawAndUpdateSideBarForLocation();


            //Set text to the location
            LocationInfo locInfo = locationPin.GetComponent<PinID>().locationInfo;
            TextMeshProUGUI locationText = sideMenuShowLocationContent.transform.Find("LocationText").GetComponent<TextMeshProUGUI>();
            locationText.text = locInfo.locationName;
            TextMeshProUGUI CoordinatesText = sideMenuShowLocationContent.transform.Find("CoordinateText").GetComponent<TextMeshProUGUI>();
            CoordinatesText.text = "Coordinates: (" + locInfo.coordinate.x + ", " + locInfo.coordinate.y + ")";
            TextMeshProUGUI DescriptionText = sideMenuShowLocationContent.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            DescriptionText.text = "Description: " + locInfo.description;

            //show image if it exists else show placeholder
            GameObject displayIMG = sideMenuShowLocationContent.transform.Find("LocationImage").gameObject;
            string imagePath = _getPathTry(locInfo.imagePath);
            Texture2D texture = new Texture2D(2, 2);
            byte[] fileData;
            if (imagePath != "")
            {
                fileData = File.ReadAllBytes(imagePath);
                texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                displayIMG.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                displayIMG.GetComponent<UnityEngine.UI.Image>().sprite = placeholderImage;
            }
            sideMenuShowLocation.SetActive(true);
        }

        public void DeleteLocation()
        {
            //delete a single location
            Debug.Log("Delete Location" + _selectedLocationInfo.locationName);
            map.DeleteLocation(_selectedLocationInfo.coordinate);

            //call for save to file
            map.SaveAll();

            //remove the pin
            _removeAllPins(locationPinsFolder);
            //reload the locations
            OnStartLoadLocations();
            //hide the side menu
            _hideSideMenu();
            _HideAllSideMenuBranches();
            CloseSideExistingMenu();




        }
        private void _hideSideMenuShowLocation()
        {
            sideMenuShowLocation.SetActive(false);
        }
        private void _removeAllPins(GameObject folder)
        {
            foreach (Transform child in folder.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }


        void addButtonClick()
        {
            _showSideMenuCreateLocation();
        }

        // Wrapper trigger popup
        public void TriggerPopup(string text, string title, string imageName)
        {
            EventManager.SetData("MODEL_POPUP", text);
            EventManager.SetData("MODEL_POPUP_TITLE", title);
            EventManager.SetData("MODEL_POPUP_IMAGE", imageName);
            EventManager.EmitEvent(eventName: "MODEL_POPUP", delay: 0, sender: gameObject);
            Debug.Log("Popup Triggered with text: " + text + " title: " + title + " imageName: " + imageName);
        }

        private string _getPathTry(string path)
        {
            Debug.Log("Path: " + path);
            if (path == "" || path == null || path == "placeholder to hold image path")
            {
                return "";
            }
            //len
            if (path.Length < 5)
            {
                TriggerPopup("File does not exist: " + path, "File Not Found", "xmark");
                return "";
            }
            //check if the path exists
            bool exists = File.Exists(path);
            if (exists)
            {
                return path;
            }

            //if it begins with /Maps, append the streaming assets path
            if (path.Substring(0, 5) == "/Maps")
            {
                path = Application.streamingAssetsPath + path;
                exists = File.Exists(path);
                if (exists)
                {
                    return path;
                }
            }

            //try to cut the path and see if it exists to only after the streaming assets path
            string newPath = path.Replace(Application.streamingAssetsPath, "");
            exists = File.Exists(newPath);
            if (exists)
            {
                return Application.streamingAssetsPath + "/" + newPath;
            }

            //try to find the streamingassets path fragment to correct for it being an absolute path
            //if path length is less than 5, it is not a valid path
            if (path.Length < 5)
            {
                //TriggerPopup("File does not exist: " + path, "File Not Found", "xmark");
                return "";
            }
            string[] pathParts = path.Split('/');
            string newPath2 = "";
            bool found = false;
            for (int i = 0; i < pathParts.Length; i++)
            {
                if (found)
                {
                    newPath2 += pathParts[i] + "/";
                }
                if (pathParts[i] == "StreamingAssets")
                {
                    found = true;
                }
            }
            newPath2 = newPath2.Substring(0, newPath2.Length - 1);
            newPath2 = Application.streamingAssetsPath + "/" + newPath2;
            exists = File.Exists(newPath2);
            if (exists)
            {
                return newPath2;
            }

            //throw an error
            TriggerPopup("File does not exist: " + path, "File Not Found", "xmark");
            return "";
        }


        /*
        Side Menu Functions
        */


        public void Toggle()
        {
            //If the side menu is active
            if (!showMorePanel.activeSelf)
            {
                //Hide the side menu
                showMorePanel.SetActive(true);
            }
        }

        public void CloseShowMore()
        {
            //If the side menu is active
            if (showMorePanel.activeSelf)
            {
                //Hide the side menu
                showMorePanel.SetActive(false);
            }
        }

        public void EditLocation()
        {
            // show the edit location menu
            _HideAllSideMenuBranches();
            sideMenuEditLocation.SetActive(true);

            // define gameobjects to change
            GameObject nameField = sideMenuEditLocation.transform.Find("Name").gameObject;
            GameObject descriptionField = sideMenuEditLocation.transform.Find("Description").gameObject;
            GameObject imageField = sideMenuEditLocation.transform.Find("DisplayIMG").gameObject;
            //GameObject isImageSelected = sideMenuEditLocation.transform.Find("IsImage").gameObject;

            // fill in the fields
            nameField.GetComponent<TMP_InputField>().text = _selectedLocationInfo.locationName;
            descriptionField.GetComponent<TMP_InputField>().text = _selectedLocationInfo.description;
            string imagePath = _getPathTry(_selectedLocationInfo.imagePath);
            Texture2D texture = new Texture2D(2, 2);
            byte[] fileData;
            if (imagePath != "")
            {
                fileData = File.ReadAllBytes(imagePath);
                texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                imageField.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                imageField.GetComponent<UnityEngine.UI.Image>().sprite = placeholderImage;
            }

            _imageToCreateWith = imageField;
            _pathToImageBeingCreated = _selectedLocationInfo.imagePath;
            imageField.SetActive(true);





        }

        public void SubmitEditLocation()
        {
            // get the fields
            GameObject nameField = sideMenuEditLocation.transform.Find("Name").gameObject;
            GameObject descriptionField = sideMenuEditLocation.transform.Find("Description").gameObject;
            GameObject imageField = sideMenuEditLocation.transform.Find("DisplayIMG").gameObject;

            // get the values
            string locationName = nameField.GetComponent<TMP_InputField>().text;
            string locationDescription = descriptionField.GetComponent<TMP_InputField>().text;
            string locationImagePath = _selectedLocationInfo.imagePath;
            if (_pathToImageBeingCreated != "")
            {
                locationImagePath = _pathToImageBeingCreated;
            }


            // update the location
            map.UpdateLocation(_selectedLocationInfo.coordinate, locationName, locationDescription, locationImagePath);

            // update the pin
            foreach (Transform child in locationPinsFolder.transform)
            {
                if (child.GetComponent<PinID>().locationInfo == _selectedLocationInfo)
                {
                    // update the pin
                    GameObject pinName = child.transform.Find("PinName").gameObject;
                    pinName.GetComponent<TextMeshProUGUI>().text = locationName;
                    //populate pinID
                    PinID id = child.GetComponent<PinID>();
                    id.locationInfo = new LocationInfo(locationName, locationDescription, _selectedLocationInfo.coordinate, locationImagePath);
                    id.type = "Location";
                    //pins of locations are green
                    child.GetComponent<Image>().color = Color.green;
                    break;
                }
            }

            //call for save to file
            map.SaveAll();

            //clear the fields for the next location
            nameField.GetComponent<TMP_InputField>().text = "";
            descriptionField.GetComponent<TMP_InputField>().text = "";
            _pathToImageBeingCreated = "";
            //hide the image
            imageField.SetActive(false);

            //remove temporary pins
            _removeAllPins(pinsFolder);

            //Close the add location canvas
            _hideSideMenu();

            //hide show more panel
            CloseSideExistingMenu();
        }

        public void CancelEditLocation()
        {
            //hide show more panel
            CloseSideExistingMenu();
            _HideAllSideMenuBranches();
            _hideSideMenu();
        }

        public void FavoriteLocation()
        {
            Debug.Log("Favorite Location");
        }

        public void CloseSideExistingMenu()
        {
            showMorePanel.SetActive(false);
            sideMenuShowLocation.SetActive(false);
            ClosepostcardMenu();
        }

        public void OpenPostcardMenu()
        {
            _showPostcardMenu();
            _drawAndUpdateSideBarForLocation();
        }

        public void ClosepostcardMenu()
        {
            _clearPostcardMenu();
            _clearPostcardPosts();
            _hidePostcardMenu();
            _conditionalClearPostcardPosts();
        }
        private void _clearPostcardPosts()
        {
            //clear the postcard menu in janky way by checking name length
            if (_postcardScrollViewContent.transform.childCount > 7)
            {
                //if there are children named postcard, remove them
                foreach (Transform child in _postcardScrollViewContent.transform)
                {
                    //janky way to detect uuid
                    if (child.gameObject.name.Length > 18)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }
            }
        }

        private void _clearPostcardMenu()
        {
            //clear the title
            _makePostCartMenuTitle.GetComponent<TMP_InputField>().text = "";
        }
        private void _conditionalClearPostcardPosts()
        {
            //clear the postcard menu
            if (_selectedLocationInfo.postCard == null || _selectedLocationInfo.postCard.posts == null || _selectedLocationInfo.postCard.posts.Count == 0)
            {
                if (_postcardScrollViewContent.transform.childCount > 7)
                {
                    //if there are children named postcard, remove them
                    foreach (Transform child in _postcardScrollViewContent.transform)
                    {
                        //janky way to detect uuid
                        if (child.gameObject.name.Length > 18)
                        {
                            _clearPostcardMenu();
                            GameObject.Destroy(child.gameObject);
                        }
                    }
                }
            }
        }

        private void _showPostcardMenu()
        {
            //show the postcard menu
            makepostcardMenu.SetActive(true);
        }

        private void _hidePostcardMenu()
        {
            //hide the postcard menu
            makepostcardMenu.SetActive(false);
        }

        public void SubmitPostcard()
        {
            //create temp postcard
            TextMediaPost _tempPost = new TextMediaPost();

            string title = _makePostCartMenuTitle.GetComponent<TMP_InputField>().text;
            //update current postcard
            for (int i = 0; i < _currentTextMediaPost.mediaComponents.Count; i++)
            {
                if (_currentTextMediaPost.mediaComponents[i].mediaType == "Text")
                {
                    TextComponent textComp = new TextComponent(_currentTextMediaPost.mediaComponents[i]);
                    GameObject textPostGameObject = GameObject.Find(textComp.uuid);
                    textComp.textContent = textPostGameObject.transform.GetChild(0).GetComponent<TMP_InputField>().text;
                    _tempPost.mediaComponents.Add(textComp);


                }
                else if (_currentTextMediaPost.mediaComponents[i].mediaType == "Image")
                {
                    ImageComponent imgComp = new ImageComponent(_currentTextMediaPost.mediaComponents[i]);
                    GameObject imgPostGameObject = GameObject.Find(imgComp.uuid);
                    imgComp.mediaPath = imgPostGameObject.transform.GetChild(2).GetComponent<TMP_Text>().text;
                    _tempPost.mediaComponents.Add(imgComp);
                }
            }

            _currentTextMediaPost.title = title;
            _tempPost.title = title;
            if (_currentTextMediaPost.title == "")
            {
                _currentTextMediaPost.title = "No Title";
            }
            _currentTextMediaPost.updateDate(System.DateTime.Now);
            _tempPost.updateDate(System.DateTime.Now);


            //get postcard that is selected
            LocationInfo locInfo = _selectedLocationInfo;
            if (locInfo == null)
            {
                return;
            }
            if (locInfo.postCard == null || locInfo.postCard.posts == null || locInfo.postCard.posts.Count == 0)
            {
                locInfo.postCard = new PostCard(_date: System.DateTime.Now);
            }
            else
            {
                locInfo.postCard.date = System.DateTime.Now;
            }
            locInfo.postCard.posts.Add(_tempPost);
            //call for save to file
            map.SaveAll();
            //Reset the new postcard
            _currentTextMediaPost = new TextMediaPost();

            //clear the postcard menu
            _clearPostcardMenu();
            _hidePostcardMenu();
            _removeAllPostsFromEdit();

            //call for update to postcard
            _drawAndUpdateSideBarForLocation();
        }

        private void _drawAndUpdateSideBarForLocation()
        {
            //Clear the postcard menu
            _clearPostcardPosts();

            //get the location info
            LocationInfo locInfo = _selectedLocationInfo;
            if (locInfo == null)
            {
                return;
            }
            if (locInfo.postCard == null || locInfo.postCard.posts == null || locInfo.postCard.posts.Count == 0)
            {
                return;
            }

            //populate the side bar
            //Set text to the location
            TextMeshProUGUI locationText = sideMenuShowLocationContent.transform.Find("LocationText").GetComponent<TextMeshProUGUI>();
            locationText.text = locInfo.locationName;
            TextMeshProUGUI CoordinatesText = sideMenuShowLocationContent.transform.Find("CoordinateText").GetComponent<TextMeshProUGUI>();
            CoordinatesText.text = "Coordinates: (" + locInfo.coordinate.x + ", " + locInfo.coordinate.y + ")";
            TextMeshProUGUI DescriptionText = sideMenuShowLocationContent.transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
            DescriptionText.text = "Description: " + locInfo.description;

            //show image if it exists else show placeholder
            GameObject displayIMG = sideMenuShowLocationContent.transform.Find("LocationImage").gameObject;
            string imagePath = _getPathTry(locInfo.imagePath);
            Texture2D texture = new Texture2D(2, 2);
            byte[] fileData;
            if (imagePath != "")
            {
                fileData = File.ReadAllBytes(imagePath);
                texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                displayIMG.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                displayIMG.GetComponent<UnityEngine.UI.Image>().sprite = placeholderImage;
            }

            sideMenuShowLocation.SetActive(true);

            //Sort the posts by sortmode
            if (_currentSortMode == SortMode.Date)
            {
                locInfo.postCard.posts.Sort((x, y) => System.DateTime.Compare(System.DateTime.Parse(x.GetDate()), System.DateTime.Parse(y.GetDate())));
            }
            else if (_currentSortMode == SortMode.Location)
            {
                locInfo.postCard.posts.Sort((x, y) => string.Compare(x.title, y.title));
            }
            else if (_currentSortMode == SortMode.Name)
            {
                locInfo.postCard.posts.Sort((x, y) => string.Compare(x.title, y.title));
            }

            //if there are postcards, draw them
            if (locInfo.postCard != null && locInfo.postCard.posts != null)
            {
                //for each post in the postcard
                foreach (TextMediaPost post in locInfo.postCard.posts)
                {
                    //WORKAROUND - create individual posts only, no postcards

                    foreach (_postMediaComponent comp in post.mediaComponents)
                    {
                        if (comp.mediaType == "Text")
                        {
                            TextComponent textComp = new TextComponent(comp);
                            //create the text content
                            GameObject textPostGameObject = Instantiate(prefabTextPostSubmitted, new Vector3(0, 0, 0), Quaternion.identity);
                            textPostGameObject.name = textComp.uuid;
                            textPostGameObject.transform.SetParent(_postcardScrollViewContent.transform);
                            textPostGameObject.transform.localScale = new Vector3(1, 1, 1);
                            GameObject posttxt = textPostGameObject.transform.Find("Scroll View").gameObject.transform.Find("Viewport").gameObject.transform.Find("Content").gameObject;
                            //posttxt contains a TextMeshPro Text UI element. Change the text to the post content


                            string poststring = textComp.textContent.Trim();
                            if (poststring.Length == 0 || poststring == "")
                            {
                                poststring = "No Description!";
                            }
                            posttxt.GetComponent<TextMeshProUGUI>().text = poststring;

                            GameObject postdata = textPostGameObject.transform.GetChild(2).gameObject;
                            string postdate = post.GetDate();
                            //get component by name
                            GameObject titleText = textPostGameObject.transform.Find("Title").gameObject;
                            titleText.GetComponent<TextMeshProUGUI>().text = post.title;
                            GameObject dateText = textPostGameObject.transform.Find("Date").gameObject;
                            dateText.GetComponent<TextMeshProUGUI>().text = "Posted on: " + postdate;

                            //assign remove component button
                            GameObject rembutton = textPostGameObject.transform.Find("DelButton").gameObject;
                            rembutton.GetComponent<Button>().onClick.AddListener(() => _removeSubmittedPostComponent(textComp.uuid));

                            ////assign edit component button
                            //GameObject editbutton = textPostGameObject.transform.Find("Edit").gameObject;
                            //editbutton.GetComponent<Button>().onClick.AddListener(() => _initEditSubmittedPostcard(textComp.uuid));
                        }
                        else if (comp.mediaType == "Image")
                        {
                            ImageComponent imageComp = new ImageComponent(comp);
                            //create the image content
                            GameObject imagePostGameObject = Instantiate(prefabImagePostSubmitted, new Vector3(0, 0, 0), Quaternion.identity);
                            imagePostGameObject.name = imageComp.uuid;
                            imagePostGameObject.transform.SetParent(_postcardScrollViewContent.transform);
                            imagePostGameObject.transform.localScale = new Vector3(1, 1, 1);
                            GameObject postimg = imagePostGameObject.transform.GetChild(1).gameObject;
                            //posttxt.GetComponent<TextMeshProUGUI>().text = "Image";
                            string postdate = post.GetDate();
                            //get component by name
                            GameObject titleText = imagePostGameObject.transform.Find("Title").gameObject;
                            titleText.GetComponent<TextMeshProUGUI>().text = post.title;
                            GameObject dateText = imagePostGameObject.transform.Find("Date").gameObject;
                            dateText.GetComponent<TextMeshProUGUI>().text = "Posted on: " + postdate;

                            //Try to load the image
                            string imagePath_corrected = _getPathTry(imageComp.mediaPath);
                            Texture2D texture_corrected = new Texture2D(2, 2);
                            byte[] fileData_corrected;
                            if (imagePath_corrected != "")
                            {
                                fileData_corrected = File.ReadAllBytes(imagePath_corrected);
                                texture_corrected.LoadImage(fileData_corrected); //..this will auto-resize the texture dimensions.
                                postimg.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(texture_corrected, new Rect(0, 0, texture_corrected.width, texture_corrected.height), new Vector2(0.5f, 0.5f));
                            }
                            else
                            {
                                postimg.GetComponent<UnityEngine.UI.Image>().sprite = placeholderImage;
                            }

                            //assign remove component button
                            GameObject rembutton = imagePostGameObject.transform.GetChild(5).gameObject;
                            rembutton.GetComponent<Button>().onClick.AddListener(() => _removeSubmittedPostComponent(imageComp.uuid));
                        }
                    }
                }





                //Create an empty object with no prefab
                //if no footer exists
                if (_postcardScrollViewContent.transform.Find("Padding") == null)
                {
                    GameObject emptyPost = new GameObject();
                    emptyPost.transform.SetParent(_postcardScrollViewContent.transform);
                    emptyPost.transform.localScale = new Vector3(1, 1, 1);
                    emptyPost.AddComponent<RectTransform>();
                    emptyPost.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                    emptyPost.name = "Padding";

                }

            }

            //trigger layout rebuild
            _postcardScrollViewContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_postcardScrollViewContent.GetComponent<RectTransform>());

        }

        public void AddImageContent()
        {
            //create code datastructure for image content
            ImageComponent imagePost = new ImageComponent();
            imagePost.mediaType = "Image";
            imagePost.mediaPath = "Path";
            string uuid = imagePost.CreateUUID();
            _currentTextMediaPost.AddImageComponent(imagePost);

            //create the image content
            GameObject imagePostGameObject = Instantiate(prefabImagePost, new Vector3(0, 0, 0), Quaternion.identity);
            imagePostGameObject.name = uuid;
            imagePostGameObject.transform.SetParent(_makePostCartMenuContentHolder.transform);
            imagePostGameObject.transform.localScale = new Vector3(2, 2, 2);

            //assign remove component button
            GameObject rembutton = imagePostGameObject.transform.Find("Remove").gameObject;
            int index = _currentTextMediaPost.mediaComponents.Count - 1;
            rembutton.GetComponent<Button>().onClick.AddListener(() => _removePostComponent(uuid));

            //Add image logic to the image content
            // need to write a new function for this
            GameObject uploadButton = imagePostGameObject.transform.Find("AddImage").gameObject.transform.Find("Upload").gameObject;
            uploadButton.GetComponent<Button>().onClick.AddListener(() => _openFileExplorerToLoadImages_PostContent(uuid));


            LayoutRebuilder.ForceRebuildLayoutImmediate(_makePostCartMenuContentHolder.GetComponent<RectTransform>());
        }

        public void AddTextContent()
        {
            //create code datastructure for text content
            TextComponent textPost = new TextComponent();
            textPost.mediaType = "Text";
            //yyyy-MM-dd HH:mm:ss
            textPost.textContent = "Description";
            string uuid = textPost.CreateUUID();
            _currentTextMediaPost.AddTextComponent(textPost);

            //create the text content
            GameObject textPostGameObject = Instantiate(prefabTextPost, new Vector3(0, 0, 0), Quaternion.identity);
            textPostGameObject.name = uuid;
            textPostGameObject.transform.SetParent(_makePostCartMenuContentHolder.transform);
            textPostGameObject.transform.localScale = new Vector3(1, 1, 1);

            //assign remove component button
            GameObject rembutton = textPostGameObject.transform.Find("InputField (TMP)").gameObject.transform.Find("Button").gameObject;
            int index = _currentTextMediaPost.mediaComponents.Count - 1;
            rembutton.GetComponent<Button>().onClick.AddListener(() => _removePostComponent(uuid));

            //trigger layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(_makePostCartMenuContentHolder.GetComponent<RectTransform>());
        }

        private void _removePostComponent(string uuid)
        {
            GameObject component = GameObject.Find(uuid); //get the component
            //remove the component from the postcard
            _currentTextMediaPost.mediaComponents.RemoveAt(_currentTextMediaPost.mediaComponents.FindIndex(x => x.uuid == uuid));
            //remove the component from the view
            Destroy(component);

            //call for save to file
            map.SaveAll();

            //trigger layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(_postcardScrollViewContent.GetComponent<RectTransform>());
        }

        private void _removeSubmittedPostComponent(string uuid)
        {
            GameObject component = GameObject.Find(uuid); //get the component
            //get location info
            LocationInfo locInfo = _selectedLocationInfo;
            if (locInfo == null)
            {
                return;
            }
            if (locInfo.postCard == null || locInfo.postCard.posts == null || locInfo.postCard.posts.Count == 0)
            {
                return;
            }
            //get posts array
            List<TextMediaPost> posts = locInfo.postCard.posts;
            //remove the component from the postcard
            foreach (TextMediaPost post in posts)
            {
                foreach (_postMediaComponent comp in post.mediaComponents)
                {
                    if (comp.uuid == uuid)
                    {
                        post.mediaComponents.Remove(comp);
                        break;
                    }
                }
            }
            //reupdate the location info
            locInfo.postCard.posts = posts;
            //call for save to file
            map.SaveAll();
            //remove the component from the view
            Destroy(component);

            //trigger layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(_postcardScrollViewContent.GetComponent<RectTransform>());
        }

        //private void _initEditSubmittedPostcard(string uuid)
        //{
        //    //get single text post and attributes
        //    TextMediaPost post = _selectedLocationInfo.postCard.posts.Find(x => x.mediaComponents.Exists(y => y.uuid == uuid));
        //    _currentTextMediaPost = post;
//
        //    //get post media component, single one
        //    _postMediaComponent comp = post.mediaComponents.Find(x => x.uuid == uuid);
//
        //    string title = post.title;
        //    _makePostCartMenuTitle.GetComponent<TMP_InputField>().text = title;
//
        //    string mediaType = comp.mediaType;
        //    string txt = comp.textContent;
//
        //    //clear the postcard menu
        //    _removeAllPostsFromEdit();
//
        //    //populate the postcard menu
        //    TextComponent textPost = new TextComponent(comp);
        //    if (mediaType == "Text")
        //    {
        //        //create the text content
        //        GameObject textPostGameObject = Instantiate(prefabTextPost, new Vector3(0, 0, 0), Quaternion.identity);
        //        textPostGameObject.name = textPost.uuid;
        //        textPostGameObject.transform.SetParent(_makePostCartMenuContentHolder.transform);
        //        textPostGameObject.transform.localScale = new Vector3(1, 1, 1);
        //        GameObject posttxt = textPostGameObject.transform.Find("InputField (TMP)").gameObject;
        //        posttxt.GetComponent<TMP_InputField>().text = txt;
//
        //        //assign remove component button
        //        GameObject rembutton = textPostGameObject.transform.Find("InputField (TMP)").gameObject.transform.Find("Button").gameObject;
        //        rembutton.GetComponent<Button>().onClick.AddListener(() => _removePostComponent(textPost.uuid));
        //    }
//
        //    //next, we open the menus
        //    _showPostcardMenu();
//
        //}


        private void _removeAllPostsFromEdit()
        {
            //clear the postcard menu
            foreach (Transform child in _makePostCartMenuContentHolder.transform)
            {
                child.gameObject.SetActive(false);
                GameObject.Destroy(child.gameObject);
            }
        }


        public void returnHome()
        {
            map.SaveAll();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        }

        public void ChangeSortModeDropdown(TMPro.TMP_Dropdown tmp_dd)
        {
            int val = tmp_dd.value;

            //set dropdown value
            _currentSortMode = (SortMode)val;

            _drawAndUpdateSideBarForLocation();
        }
    }

}
