using System;
using System.Collections.Generic;
using System.Linq;

namespace FindContainer
{
    /// <summary>
    /// A 3D bin packing algorithm originally ported from https://github.com/keremdemirer/3dbinpackingjs,
    /// which itself was a JavaScript port of https://github.com/wknechtel/3d-bin-pack/,
    /// which is a C reconstruction of a novel algorithm developed in a U.S. Air Force master's thesis by Erhan Baltacioglu in 2001.
    /// </summary>
    public class EbAfit
    {
        /// <summary>
        /// Runs the packing algorithm.
        /// </summary>
        /// <param name="container">The container to pack items into.</param>
        /// <param name="items">The items to pack.</param>
        /// <returns>The bin packing result.</returns>
        public AlgorithmPackingResult RunPackingAlgorithm(Container container, List<Item> items)
        {
            this._container = container;

            Initialize(items);
            ExecutePackingAlgorithmVariants();
            Report();

            AlgorithmPackingResult result = new AlgorithmPackingResult();

            for (int i = 1; i <= _itemsToPackCount; i++)
            {
                _itemsToPack[i].Quantity = 1;

                if (_itemsToPack[i].IsPacked)
                {
                    result.PackedItems.Add(_itemsToPack[i]);
                }
                else
                {
                    result.UnpackedItems.Add(_itemsToPack[i]);
                }
            }

            result.PercentContainerVolumePacked = _percentageContainerUsed;
            result.PackedItemCount = _packedItemCount;

            if (result.UnpackedItems.Count == 0)
            {
                result.IsCompletePack = true;
            }

            return result;
        }

        private Container _container;
        private List<Item> _itemsToPack;
        private List<Layer> _layers;
        private ContainerPackingResult _result;

        private ScrapPad _scrapfirst;
        private ScrapPad _scrapmemb;
        private ScrapPad _smallestZ;
        private ScrapPad _trash;

        private bool _evened;
        private bool _hundredPercentPacked = false;
        private bool _layerDone;
        private bool _packing;
        private bool _packingBest = false;
        private bool _quit = false;
        private bool _unpacked;

        private int _bboxi;
        private int _bestIteration;
        private int _bestPackedItemCount;
        private int _bestVariant;
        private int _boxi;
        private int _cboxi;
        private int _itelayer;
        private int _iterationsCount;
        private int _layerListLen;
        private int _layersIndex;
        private int _n;
        private int _packedItemCount;
        private int _variant;
        private int _x;

        private decimal _bbfx;
        private decimal _bbfy;
        private decimal _bbfz;
        private decimal _bboxx;
        private decimal _bboxy;
        private decimal _bboxz;
        private decimal _bestVolume;
        private decimal _bfx;
        private decimal _bfy;
        private decimal _bfz;
        private decimal _boxx;
        private decimal _boxy;
        private decimal _boxz;
        private decimal _cboxx;
        private decimal _cboxy;
        private decimal _cboxz;
        private decimal _layerinlayer;
        private decimal _layerThickness;
        private decimal _lilz;
        private decimal _packedVolume;
        private decimal _packedy;
        private decimal _percentagePackedItemsByVolume;
        private decimal _percentageContainerUsed;
        private decimal _prelayer;
        private decimal _prepackedy;
        private decimal _preremainpy;
        private decimal _px;
        private decimal _py;
        private decimal _pz;
        private decimal _remainpy;
        private decimal _remainpz;
        private decimal _strcox;
        private decimal _strcoy;
        private decimal _strcoz;
        private decimal _strpackx;
        private decimal _strpacky;
        private decimal _strpackz;
        private decimal _itemsToPackCount;
        private decimal _totalItemVolume;
        private decimal _totalContainerVolume;

        /// <summary>
        /// Analyzes each unpacked box to find the best fitting one to the empty space given.
        /// </summary>
        private void AnalyzeBox(decimal hmx, decimal hy, decimal hmy, decimal hz, decimal hmz, decimal dim1, decimal dim2, decimal dim3)
        {
            if (dim1 <= hmx && dim2 <= hmy && dim3 <= hmz)
            {
                if (dim2 <= hy)
                {
                    if (hy - dim2 < _bfy)
                    {
                        _boxx = dim1;
                        _boxy = dim2;
                        _boxz = dim3;
                        _bfx = hmx - dim1;
                        _bfy = hy - dim2;
                        _bfz = Math.Abs(hz - dim3);
                        _boxi = _x;
                    }
                    else if (hy - dim2 == _bfy && hmx - dim1 < _bfx)
                    {
                        _boxx = dim1;
                        _boxy = dim2;
                        _boxz = dim3;
                        _bfx = hmx - dim1;
                        _bfy = hy - dim2;
                        _bfz = Math.Abs(hz - dim3);
                        _boxi = _x;
                    }
                    else if (hy - dim2 == _bfy && hmx - dim1 == _bfx && Math.Abs(hz - dim3) < _bfz)
                    {
                        _boxx = dim1;
                        _boxy = dim2;
                        _boxz = dim3;
                        _bfx = hmx - dim1;
                        _bfy = hy - dim2;
                        _bfz = Math.Abs(hz - dim3);
                        _boxi = _x;
                    }
                }
                else
                {
                    if (dim2 - hy < _bbfy)
                    {
                        _bboxx = dim1;
                        _bboxy = dim2;
                        _bboxz = dim3;
                        _bbfx = hmx - dim1;
                        _bbfy = dim2 - hy;
                        _bbfz = Math.Abs(hz - dim3);
                        _bboxi = _x;
                    }
                    else if (dim2 - hy == _bbfy && hmx - dim1 < _bbfx)
                    {
                        _bboxx = dim1;
                        _bboxy = dim2;
                        _bboxz = dim3;
                        _bbfx = hmx - dim1;
                        _bbfy = dim2 - hy;
                        _bbfz = Math.Abs(hz - dim3);
                        _bboxi = _x;
                    }
                    else if (dim2 - hy == _bbfy && hmx - dim1 == _bbfx && Math.Abs(hz - dim3) < _bbfz)
                    {
                        _bboxx = dim1;
                        _bboxy = dim2;
                        _bboxz = dim3;
                        _bbfx = hmx - dim1;
                        _bbfy = dim2 - hy;
                        _bbfz = Math.Abs(hz - dim3);
                        _bboxi = _x;
                    }
                }
            }
        }

        /// <summary>
        /// After finding each box, the candidate boxes and the condition of the layer are examined.
        /// </summary>
        private void CheckFound()
        {
            _evened = false;

            if (_boxi != 0)
            {
                _cboxi = _boxi;
                _cboxx = _boxx;
                _cboxy = _boxy;
                _cboxz = _boxz;
            }
            else
            {
                if ((_bboxi > 0) && (_layerinlayer != 0 || (_smallestZ.Pre == null && _smallestZ.Post == null)))
                {
                    if (_layerinlayer == 0)
                    {
                        _prelayer = _layerThickness;
                        _lilz = _smallestZ.CumZ;
                    }

                    _cboxi = _bboxi;
                    _cboxx = _bboxx;
                    _cboxy = _bboxy;
                    _cboxz = _bboxz;
                    _layerinlayer = _layerinlayer + _bboxy - _layerThickness;
                    _layerThickness = _bboxy;
                }
                else
                {
                    if (_smallestZ.Pre == null && _smallestZ.Post == null)
                    {
                        _layerDone = true;
                    }
                    else
                    {
                        _evened = true;

                        if (_smallestZ.Pre == null)
                        {
                            _trash = _smallestZ.Post;
                            _smallestZ.CumX = _smallestZ.Post.CumX;
                            _smallestZ.CumZ = _smallestZ.Post.CumZ;
                            _smallestZ.Post = _smallestZ.Post.Post;
                            if (_smallestZ.Post != null)
                            {
                                _smallestZ.Post.Pre = _smallestZ;
                            }
                        }
                        else if (_smallestZ.Post == null)
                        {
                            _smallestZ.Pre.Post = null;
                            _smallestZ.Pre.CumX = _smallestZ.CumX;
                        }
                        else
                        {
                            if (_smallestZ.Pre.CumZ == _smallestZ.Post.CumZ)
                            {
                                _smallestZ.Pre.Post = _smallestZ.Post.Post;

                                if (_smallestZ.Post.Post != null)
                                {
                                    _smallestZ.Post.Post.Pre = _smallestZ.Pre;
                                }

                                _smallestZ.Pre.CumX = _smallestZ.Post.CumX;
                            }
                            else
                            {
                                _smallestZ.Pre.Post = _smallestZ.Post;
                                _smallestZ.Post.Pre = _smallestZ.Pre;

                                if (_smallestZ.Pre.CumZ < _smallestZ.Post.CumZ)
                                {
                                    _smallestZ.Pre.CumX = _smallestZ.CumX;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Executes the packing algorithm variants.
        /// </summary>
        private void ExecutePackingAlgorithmVariants()
        {
            for (_variant = 1; (_variant <= 6) && !_quit; _variant++)
            {
                switch (_variant)
                {
                    case 1:
                        _px = _container.Length; _py = _container.Height; _pz = _container.Width;
                        break;

                    case 2:
                        _px = _container.Width; _py = _container.Height; _pz = _container.Length;
                        break;

                    case 3:
                        _px = _container.Width; _py = _container.Length; _pz = _container.Height;
                        break;

                    case 4:
                        _px = _container.Height; _py = _container.Length; _pz = _container.Width;
                        break;

                    case 5:
                        _px = _container.Length; _py = _container.Width; _pz = _container.Height;
                        break;

                    case 6:
                        _px = _container.Height; _py = _container.Width; _pz = _container.Length;
                        break;
                }

                _layers.Add(new Layer { LayerEval = -1 });
                ListCanditLayers();
                _layers = _layers.OrderBy(l => l.LayerEval).ToList();

                for (_layersIndex = 1; (_layersIndex <= _layerListLen) && !_quit; _layersIndex++)
                {
                    ++_iterationsCount;

                    //printf("VARIANT: " + variant + "; ITERATION (TOTAL): " + itenum + "; BEST SO FAR: " + percentageused + " %%;");
                    _packedVolume = 0.0M;
                    _packedy = 0;
                    _packing = true;
                    _layerThickness = _layers[_layersIndex].LayerDim;
                    _itelayer = _layersIndex;
                    _remainpy = _py;
                    _remainpz = _pz;
                    _packedItemCount = 0;

                    for (_x = 1; _x <= _itemsToPackCount; _x++)
                    {
                        _itemsToPack[_x].IsPacked = false;
                    }

                    do
                    {
                        _layerinlayer = 0;
                        _layerDone = false;

                        PackLayer();

                        _packedy = _packedy + _layerThickness;
                        _remainpy = _py - _packedy;

                        if (_layerinlayer != 0 && !_quit)
                        {
                            _prepackedy = _packedy;
                            _preremainpy = _remainpy;
                            _remainpy = _layerThickness - _prelayer;
                            _packedy = _packedy - _layerThickness + _prelayer;
                            _remainpz = _lilz;
                            _layerThickness = _layerinlayer;
                            _layerDone = false;

                            PackLayer();

                            _packedy = _prepackedy;
                            _remainpy = _preremainpy;
                            _remainpz = _pz;
                        }

                        FindLayer(_remainpy);
                    } while (_packing && !_quit);

                    if ((_packedVolume > _bestVolume) && !_quit)
                    {
                        _bestVolume = _packedVolume;
                        _bestVariant = _variant;
                        _bestIteration = _itelayer;
                        _bestPackedItemCount = _packedItemCount;
                    }

                    if (_hundredPercentPacked) break;

                    _percentageContainerUsed = _bestVolume * 100 / _totalContainerVolume;
                }

                if (_hundredPercentPacked) break;

                if ((_container.Length == _container.Height) && (_container.Height == _container.Width)) _variant = 6;
            }
        }

        /// <summary>
        /// Finds the most proper boxes by looking at all six possible orientations,
        /// empty space given, adjacent boxes, and pallet limits.
        /// </summary>
        private void FindBox(decimal hmx, decimal hy, decimal hmy, decimal hz, decimal hmz)
        {
            int y;
            _bfx = 32767;
            _bfy = 32767;
            _bfz = 32767;
            _bbfx = 32767;
            _bbfy = 32767;
            _bbfz = 32767;
            _boxi = 0;
            _bboxi = 0;

            for (y = 1; y <= _itemsToPackCount; y = y + _itemsToPack[y].Quantity)
            {
                for (_x = y; _x < _x + _itemsToPack[y].Quantity - 1; _x++)
                {
                    if (!_itemsToPack[_x].IsPacked) break;
                }

                if (_itemsToPack[_x].IsPacked) continue;

                if (_x > _itemsToPackCount) return;

                AnalyzeBox(hmx, hy, hmy, hz, hmz, _itemsToPack[_x].Dim1, _itemsToPack[_x].Dim2, _itemsToPack[_x].Dim3);

                if ((_itemsToPack[_x].Dim1 == _itemsToPack[_x].Dim3) && (_itemsToPack[_x].Dim3 == _itemsToPack[_x].Dim2)) continue;

                AnalyzeBox(hmx, hy, hmy, hz, hmz, _itemsToPack[_x].Dim1, _itemsToPack[_x].Dim3, _itemsToPack[_x].Dim2);
                AnalyzeBox(hmx, hy, hmy, hz, hmz, _itemsToPack[_x].Dim2, _itemsToPack[_x].Dim1, _itemsToPack[_x].Dim3);
                AnalyzeBox(hmx, hy, hmy, hz, hmz, _itemsToPack[_x].Dim2, _itemsToPack[_x].Dim3, _itemsToPack[_x].Dim1);
                AnalyzeBox(hmx, hy, hmy, hz, hmz, _itemsToPack[_x].Dim3, _itemsToPack[_x].Dim1, _itemsToPack[_x].Dim2);
                AnalyzeBox(hmx, hy, hmy, hz, hmz, _itemsToPack[_x].Dim3, _itemsToPack[_x].Dim2, _itemsToPack[_x].Dim1);
            }
        }

        /// <summary>
        /// Finds the most proper layer height by looking at the unpacked boxes and the remaining empty space available.
        /// </summary>
        private void FindLayer(decimal thickness)
        {
            decimal exdim = 0;
            decimal dimdif;
            decimal dimen2 = 0;
            decimal dimen3 = 0;
            int y;
            int z;
            decimal layereval;
            decimal eval;
            _layerThickness = 0;
            eval = 1000000;

            for (_x = 1; _x <= _itemsToPackCount; _x++)
            {
                if (_itemsToPack[_x].IsPacked) continue;

                for (y = 1; y <= 3; y++)
                {
                    switch (y)
                    {
                        case 1:
                            exdim = _itemsToPack[_x].Dim1;
                            dimen2 = _itemsToPack[_x].Dim2;
                            dimen3 = _itemsToPack[_x].Dim3;
                            break;

                        case 2:
                            exdim = _itemsToPack[_x].Dim2;
                            dimen2 = _itemsToPack[_x].Dim1;
                            dimen3 = _itemsToPack[_x].Dim3;
                            break;

                        case 3:
                            exdim = _itemsToPack[_x].Dim3;
                            dimen2 = _itemsToPack[_x].Dim1;
                            dimen3 = _itemsToPack[_x].Dim2;
                            break;
                    }

                    layereval = 0;

                    if ((exdim <= thickness) && (((dimen2 <= _px) && (dimen3 <= _pz)) || ((dimen3 <= _px) && (dimen2 <= _pz))))
                    {
                        for (z = 1; z <= _itemsToPackCount; z++)
                        {
                            if (!(_x == z) && !(_itemsToPack[z].IsPacked))
                            {
                                dimdif = Math.Abs(exdim - _itemsToPack[z].Dim1);

                                if (Math.Abs(exdim - _itemsToPack[z].Dim2) < dimdif)
                                {
                                    dimdif = Math.Abs(exdim - _itemsToPack[z].Dim2);
                                }

                                if (Math.Abs(exdim - _itemsToPack[z].Dim3) < dimdif)
                                {
                                    dimdif = Math.Abs(exdim - _itemsToPack[z].Dim3);
                                }

                                layereval = layereval + dimdif;
                            }
                        }

                        if (layereval < eval)
                        {
                            eval = layereval;
                            _layerThickness = exdim;
                        }
                    }
                }
            }

            if (_layerThickness == 0 || _layerThickness > _remainpy) _packing = false;
        }

        /// <summary>
        /// Finds the first to be packed gap in the layer edge.
        /// </summary>
        private void FindSmallestZ()
        {
            _scrapmemb = _scrapfirst;
            _smallestZ = _scrapmemb;

            while (_scrapmemb.Post != null)
            {
                if (_scrapmemb.Post.CumZ < _smallestZ.CumZ)
                {
                    _smallestZ = _scrapmemb.Post;
                }

                _scrapmemb = _scrapmemb.Post;
            }
        }

        /// <summary>
        /// Data for the visualization program is written to the "visudat" file and
        /// the list of unpacked boxes is merged to the end of the report file.
        /// </summary>
        private void GraphUnpackedOut()
        {
            int n = 0;

            if (!_unpacked)
            {
                _strcox = _itemsToPack[_cboxi].CoordX;
                _strcoy = _itemsToPack[_cboxi].CoordY;
                _strcoz = _itemsToPack[_cboxi].CoordZ;
                _strpackx = _itemsToPack[_cboxi].PackDimX;
                _strpacky = _itemsToPack[_cboxi].PackDimY;
                _strpackz = _itemsToPack[_cboxi].PackDimZ;
            }
            else
            {
                n = _cboxi;
                _strpackx = _itemsToPack[_cboxi].Dim1;
                _strpacky = _itemsToPack[_cboxi].Dim2;
                _strpackz = _itemsToPack[_cboxi].Dim3;
            }
            if (!_unpacked)
            {
                //Print(strcox, strcoy, strcoz, strpackx, strpacky, strpackz);
            }
            else
            {
                //Print(n, _strpackx, _strpacky, _strpackz);
            }
        }

        /// <summary>
        /// Initializes everything.
        /// </summary>
        private void Initialize(List<Item> items)
        {
            _itemsToPack = new List<Item>();
            _result = new ContainerPackingResult();

            // The original code uses 1-based indexing everywhere. This fake entry is added to the beginning
            // of the list to make that possible.
            _itemsToPack.Add(new Item(0, 0, 0, 0, 0));

            _layers = new List<Layer>();
            _itemsToPackCount = 0;

            foreach (Item item in items)
            {
                for (int i = 1; i <= item.Quantity; i++)
                {
                    Item newItem = new Item(item.Id, item.Dim1, item.Dim2, item.Dim3, item.Quantity);
                    _itemsToPack.Add(newItem);
                }

                _itemsToPackCount += item.Quantity;
            }

            _itemsToPack.Add(new Item(0, 0, 0, 0, 0));

            _totalContainerVolume = _container.Length * _container.Height * _container.Width;
            _totalItemVolume = 0.0M;

            for (_x = 1; _x <= _itemsToPackCount; _x++)
            {
                _totalItemVolume = _totalItemVolume + _itemsToPack[_x].Volume;
            }

            _scrapfirst = new ScrapPad();

            _scrapfirst.Pre = null;
            _scrapfirst.Post = null;
            _bestVolume = 0.0M;
            _packingBest = false;
            _hundredPercentPacked = false;
            _iterationsCount = 0;
            _quit = false;
        }

        /// <summary>
        /// Lists all possible layer heights by giving a weight value to each of them.
        /// </summary>
        private void ListCanditLayers()
        {
            bool same;
            decimal exdim = 0;
            decimal dimdif;
            decimal dimen2 = 0;
            decimal dimen3 = 0;
            int y;
            int z;
            int k;
            decimal layereval;

            _layerListLen = 0;

            for (_x = 1; _x <= _itemsToPackCount; _x++)
            {
                for (y = 1; y <= 3; y++)
                {
                    switch (y)
                    {
                        case 1:
                            exdim = _itemsToPack[_x].Dim1;
                            dimen2 = _itemsToPack[_x].Dim2;
                            dimen3 = _itemsToPack[_x].Dim3;
                            break;

                        case 2:
                            exdim = _itemsToPack[_x].Dim2;
                            dimen2 = _itemsToPack[_x].Dim1;
                            dimen3 = _itemsToPack[_x].Dim3;
                            break;

                        case 3:
                            exdim = _itemsToPack[_x].Dim3;
                            dimen2 = _itemsToPack[_x].Dim1;
                            dimen3 = _itemsToPack[_x].Dim2;
                            break;
                    }

                    if ((exdim > _py) || (((dimen2 > _px) || (dimen3 > _pz)) && ((dimen3 > _px) || (dimen2 > _pz)))) continue;

                    same = false;

                    for (k = 1; k <= _layerListLen; k++)
                    {
                        if (exdim == _layers[k].LayerDim)
                        {
                            same = true;
                            continue;
                        }
                    }

                    if (same) continue;

                    layereval = 0;

                    for (z = 1; z <= _itemsToPackCount; z++)
                    {
                        if (!(_x == z))
                        {
                            dimdif = Math.Abs(exdim - _itemsToPack[z].Dim1);

                            if (Math.Abs(exdim - _itemsToPack[z].Dim2) < dimdif)
                            {
                                dimdif = Math.Abs(exdim - _itemsToPack[z].Dim2);
                            }
                            if (Math.Abs(exdim - _itemsToPack[z].Dim3) < dimdif)
                            {
                                dimdif = Math.Abs(exdim - _itemsToPack[z].Dim3);
                            }
                            layereval = layereval + dimdif;
                        }
                    }

                    _layerListLen++;

                    _layers.Add(new Layer());
                    _layers[_layerListLen].LayerEval = layereval;
                    _layers[_layerListLen].LayerDim = exdim;
                }
            }
        }

        /// <summary>
        /// Transforms the found coordinate system to the one entered by the user and writes them
        /// to the report file.
        /// </summary>
        private void OutputBoxList()
        {
            dynamic x = 0;
            dynamic y = 0;
            dynamic z = 0;
            dynamic bx = 0;
            dynamic by = 0;
            dynamic bz = 0;

            switch (_bestVariant)
            {
                case 1:
                    x = _itemsToPack[_cboxi].CoordX;
                    y = _itemsToPack[_cboxi].CoordY;
                    z = _itemsToPack[_cboxi].CoordZ;
                    bx = _itemsToPack[_cboxi].PackDimX;
                    by = _itemsToPack[_cboxi].PackDimY;
                    bz = _itemsToPack[_cboxi].PackDimZ;
                    break;

                case 2:
                    x = _itemsToPack[_cboxi].CoordZ;
                    y = _itemsToPack[_cboxi].CoordY;
                    z = _itemsToPack[_cboxi].CoordX;
                    bx = _itemsToPack[_cboxi].PackDimZ;
                    by = _itemsToPack[_cboxi].PackDimY;
                    bz = _itemsToPack[_cboxi].PackDimX;
                    break;

                case 3:
                    x = _itemsToPack[_cboxi].CoordY;
                    y = _itemsToPack[_cboxi].CoordZ;
                    z = _itemsToPack[_cboxi].CoordX;
                    bx = _itemsToPack[_cboxi].PackDimY;
                    by = _itemsToPack[_cboxi].PackDimZ;
                    bz = _itemsToPack[_cboxi].PackDimX;
                    break;

                case 4:
                    x = _itemsToPack[_cboxi].CoordY;
                    y = _itemsToPack[_cboxi].CoordX;
                    z = _itemsToPack[_cboxi].CoordZ;
                    bx = _itemsToPack[_cboxi].PackDimY;
                    by = _itemsToPack[_cboxi].PackDimX;
                    bz = _itemsToPack[_cboxi].PackDimZ;
                    break;

                case 5:
                    x = _itemsToPack[_cboxi].CoordX;
                    y = _itemsToPack[_cboxi].CoordZ;
                    z = _itemsToPack[_cboxi].CoordY;
                    bx = _itemsToPack[_cboxi].PackDimX;
                    by = _itemsToPack[_cboxi].PackDimZ;
                    bz = _itemsToPack[_cboxi].PackDimY;
                    break;

                case 6:
                    x = _itemsToPack[_cboxi].CoordZ;
                    y = _itemsToPack[_cboxi].CoordX;
                    z = _itemsToPack[_cboxi].CoordY;
                    bx = _itemsToPack[_cboxi].PackDimZ;
                    by = _itemsToPack[_cboxi].PackDimX;
                    bz = _itemsToPack[_cboxi].PackDimY;
                    break;
            }

            var strx = _cboxi;
            var strpackst = _itemsToPack[_cboxi].IsPacked;
            var strdim1 = _itemsToPack[_cboxi].Dim1;
            var strdim2 = _itemsToPack[_cboxi].Dim2;
            var strdim3 = _itemsToPack[_cboxi].Dim3;
            var strcox = x;
            var strcoy = y;
            var strcoz = z;
            var strpackx = bx;
            var strpacky = @by;
            var strpackz = bz;

            _itemsToPack[_cboxi].CoordX = x;
            _itemsToPack[_cboxi].CoordY = y;
            _itemsToPack[_cboxi].CoordZ = z;
            _itemsToPack[_cboxi].PackDimX = bx;
            _itemsToPack[_cboxi].PackDimY = by;
            _itemsToPack[_cboxi].PackDimZ = bz;

            //Print(strx, strpackst, strdim1, strdim2, strdim3, strcox, strcoy, strcoz, strpackx, strpacky, strpackz);
        }

        /// <summary>
        /// Packs the boxes found and arranges all variables and records properly.
        /// </summary>
        private void PackLayer()
        {
            if (_layerThickness == 0)
            {
                _packing = false;
                return;
            }

            _scrapfirst.CumX = _px;
            _scrapfirst.CumZ = 0;

            for (; !_quit;)
            {
                FindSmallestZ();

                decimal lenx;
                decimal lpz;
                if ((_smallestZ.Pre == null) && (_smallestZ.Post == null))
                {
                    //*** SITUATION-1: NO BOXES ON THE RIGHT AND LEFT SIDES ***

                    lenx = _smallestZ.CumX;
                    lpz = _remainpz - _smallestZ.CumZ;
                    FindBox(lenx, _layerThickness, _remainpy, lpz, lpz);
                    CheckFound();

                    if (_layerDone) break;
                    if (_evened) continue;

                    _itemsToPack[_cboxi].CoordX = 0;
                    _itemsToPack[_cboxi].CoordY = _packedy;
                    _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                    if (_cboxx == _smallestZ.CumX)
                    {
                        _smallestZ.CumZ = _smallestZ.CumZ + _cboxz;
                    }
                    else
                    {
                        _smallestZ.Post = new ScrapPad();

                        _smallestZ.Post.Post = null;
                        _smallestZ.Post.Pre = _smallestZ;
                        _smallestZ.Post.CumX = _smallestZ.CumX;
                        _smallestZ.Post.CumZ = _smallestZ.CumZ;
                        _smallestZ.CumX = _cboxx;
                        _smallestZ.CumZ = _smallestZ.CumZ + _cboxz;
                    }

                    VolumeCheck();
                }
                else
                {
                    decimal lenz;
                    if (_smallestZ.Pre == null)
                    {
                        //*** SITUATION-2: NO BOXES ON THE LEFT SIDE ***

                        lenx = _smallestZ.CumX;
                        lenz = _smallestZ.Post.CumZ - _smallestZ.CumZ;
                        lpz = _remainpz - _smallestZ.CumZ;
                        FindBox(lenx, _layerThickness, _remainpy, lenz, lpz);
                        CheckFound();

                        if (_layerDone) break;
                        if (_evened) continue;

                        _itemsToPack[_cboxi].CoordY = _packedy;
                        _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                        if (_cboxx == _smallestZ.CumX)
                        {
                            _itemsToPack[_cboxi].CoordX = 0;

                            if (_smallestZ.CumZ + _cboxz == _smallestZ.Post.CumZ)
                            {
                                _smallestZ.CumZ = _smallestZ.Post.CumZ;
                                _smallestZ.CumX = _smallestZ.Post.CumX;
                                _trash = _smallestZ.Post;
                                _smallestZ.Post = _smallestZ.Post.Post;

                                if (_smallestZ.Post != null)
                                {
                                    _smallestZ.Post.Pre = _smallestZ;
                                }
                            }
                            else
                            {
                                _smallestZ.CumZ = _smallestZ.CumZ + _cboxz;
                            }
                        }
                        else
                        {
                            _itemsToPack[_cboxi].CoordX = _smallestZ.CumX - _cboxx;

                            if (_smallestZ.CumZ + _cboxz == _smallestZ.Post.CumZ)
                            {
                                _smallestZ.CumX = _smallestZ.CumX - _cboxx;
                            }
                            else
                            {
                                _smallestZ.Post.Pre = new ScrapPad();

                                _smallestZ.Post.Pre.Post = _smallestZ.Post;
                                _smallestZ.Post.Pre.Pre = _smallestZ;
                                _smallestZ.Post = _smallestZ.Post.Pre;
                                _smallestZ.Post.CumX = _smallestZ.CumX;
                                _smallestZ.CumX = _smallestZ.CumX - _cboxx;
                                _smallestZ.Post.CumZ = _smallestZ.CumZ + _cboxz;
                            }
                        }

                        VolumeCheck();
                    }
                    else if (_smallestZ.Post == null)
                    {
                        //*** SITUATION-3: NO BOXES ON THE RIGHT SIDE ***

                        lenx = _smallestZ.CumX - _smallestZ.Pre.CumX;
                        lenz = _smallestZ.Pre.CumZ - _smallestZ.CumZ;
                        lpz = _remainpz - _smallestZ.CumZ;
                        FindBox(lenx, _layerThickness, _remainpy, lenz, lpz);
                        CheckFound();

                        if (_layerDone) break;
                        if (_evened) continue;

                        _itemsToPack[_cboxi].CoordY = _packedy;
                        _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                        _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;

                        if (_cboxx == _smallestZ.CumX - _smallestZ.Pre.CumX)
                        {
                            if (_smallestZ.CumZ + _cboxz == _smallestZ.Pre.CumZ)
                            {
                                _smallestZ.Pre.CumX = _smallestZ.CumX;
                                _smallestZ.Pre.Post = null;
                            }
                            else
                            {
                                _smallestZ.CumZ = _smallestZ.CumZ + _cboxz;
                            }
                        }
                        else
                        {
                            if (_smallestZ.CumZ + _cboxz == _smallestZ.Pre.CumZ)
                            {
                                _smallestZ.Pre.CumX = _smallestZ.Pre.CumX + _cboxx;
                            }
                            else
                            {
                                _smallestZ.Pre.Post = new ScrapPad();

                                _smallestZ.Pre.Post.Pre = _smallestZ.Pre;
                                _smallestZ.Pre.Post.Post = _smallestZ;
                                _smallestZ.Pre = _smallestZ.Pre.Post;
                                _smallestZ.Pre.CumX = _smallestZ.Pre.Pre.CumX + _cboxx;
                                _smallestZ.Pre.CumZ = _smallestZ.CumZ + _cboxz;
                            }
                        }

                        VolumeCheck();
                    }
                    else if (_smallestZ.Pre.CumZ == _smallestZ.Post.CumZ)
                    {
                        //*** SITUATION-4: THERE ARE BOXES ON BOTH OF THE SIDES ***

                        //*** SUBSITUATION-4A: SIDES ARE EQUAL TO EACH OTHER ***

                        lenx = _smallestZ.CumX - _smallestZ.Pre.CumX;
                        lenz = _smallestZ.Pre.CumZ - _smallestZ.CumZ;
                        lpz = _remainpz - _smallestZ.CumZ;

                        FindBox(lenx, _layerThickness, _remainpy, lenz, lpz);
                        CheckFound();

                        if (_layerDone) break;
                        if (_evened) continue;

                        _itemsToPack[_cboxi].CoordY = _packedy;
                        _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;

                        if (_cboxx == _smallestZ.CumX - _smallestZ.Pre.CumX)
                        {
                            _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;

                            if (_smallestZ.CumZ + _cboxz == _smallestZ.Post.CumZ)
                            {
                                _smallestZ.Pre.CumX = _smallestZ.Post.CumX;

                                if (_smallestZ.Post.Post != null)
                                {
                                    _smallestZ.Pre.Post = _smallestZ.Post.Post;
                                    _smallestZ.Post.Post.Pre = _smallestZ.Pre;
                                }
                                else
                                {
                                    _smallestZ.Pre.Post = null;
                                }
                            }
                            else
                            {
                                _smallestZ.CumZ = _smallestZ.CumZ + _cboxz;
                            }
                        }
                        else if (_smallestZ.Pre.CumX < _px - _smallestZ.CumX)
                        {
                            if (_smallestZ.CumZ + _cboxz == _smallestZ.Pre.CumZ)
                            {
                                _smallestZ.CumX = _smallestZ.CumX - _cboxx;
                                _itemsToPack[_cboxi].CoordX = _smallestZ.CumX - _cboxx;
                            }
                            else
                            {
                                _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;
                                _smallestZ.Pre.Post = new ScrapPad();

                                _smallestZ.Pre.Post.Pre = _smallestZ.Pre;
                                _smallestZ.Pre.Post.Post = _smallestZ;
                                _smallestZ.Pre = _smallestZ.Pre.Post;
                                _smallestZ.Pre.CumX = _smallestZ.Pre.Pre.CumX + _cboxx;
                                _smallestZ.Pre.CumZ = _smallestZ.CumZ + _cboxz;
                            }
                        }
                        else
                        {
                            if (_smallestZ.CumZ + _cboxz == _smallestZ.Pre.CumZ)
                            {
                                _smallestZ.Pre.CumX = _smallestZ.Pre.CumX + _cboxx;
                                _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;
                            }
                            else
                            {
                                _itemsToPack[_cboxi].CoordX = _smallestZ.CumX - _cboxx;
                                _smallestZ.Post.Pre = new ScrapPad();

                                _smallestZ.Post.Pre.Post = _smallestZ.Post;
                                _smallestZ.Post.Pre.Pre = _smallestZ;
                                _smallestZ.Post = _smallestZ.Post.Pre;
                                _smallestZ.Post.CumX = _smallestZ.CumX;
                                _smallestZ.Post.CumZ = _smallestZ.CumZ + _cboxz;
                                _smallestZ.CumX = _smallestZ.CumX - _cboxx;
                            }
                        }

                        VolumeCheck();
                    }
                    else
                    {
                        //*** SUBSITUATION-4B: SIDES ARE NOT EQUAL TO EACH OTHER ***

                        lenx = _smallestZ.CumX - _smallestZ.Pre.CumX;
                        lenz = _smallestZ.Pre.CumZ - _smallestZ.CumZ;
                        lpz = _remainpz - _smallestZ.CumZ;
                        FindBox(lenx, _layerThickness, _remainpy, lenz, lpz);
                        CheckFound();

                        if (_layerDone) break;
                        if (_evened) continue;

                        _itemsToPack[_cboxi].CoordY = _packedy;
                        _itemsToPack[_cboxi].CoordZ = _smallestZ.CumZ;
                        _itemsToPack[_cboxi].CoordX = _smallestZ.Pre.CumX;

                        if (_cboxx == (_smallestZ.CumX - _smallestZ.Pre.CumX))
                        {
                            if ((_smallestZ.CumZ + _cboxz) == _smallestZ.Pre.CumZ)
                            {
                                _smallestZ.Pre.CumX = _smallestZ.CumX;
                                _smallestZ.Pre.Post = _smallestZ.Post;
                                _smallestZ.Post.Pre = _smallestZ.Pre;
                            }
                            else
                            {
                                _smallestZ.CumZ = _smallestZ.CumZ + _cboxz;
                            }
                        }
                        else
                        {
                            if ((_smallestZ.CumZ + _cboxz) == _smallestZ.Pre.CumZ)
                            {
                                _smallestZ.Pre.CumX = _smallestZ.Pre.CumX + _cboxx;
                            }
                            else if (_smallestZ.CumZ + _cboxz == _smallestZ.Post.CumZ)
                            {
                                _itemsToPack[_cboxi].CoordX = _smallestZ.CumX - _cboxx;
                                _smallestZ.CumX = _smallestZ.CumX - _cboxx;
                            }
                            else
                            {
                                _smallestZ.Pre.Post = new ScrapPad();

                                _smallestZ.Pre.Post.Pre = _smallestZ.Pre;
                                _smallestZ.Pre.Post.Post = _smallestZ;
                                _smallestZ.Pre = _smallestZ.Pre.Post;
                                _smallestZ.Pre.CumX = _smallestZ.Pre.Pre.CumX + _cboxx;
                                _smallestZ.Pre.CumZ = _smallestZ.CumZ + _cboxz;
                            }
                        }

                        VolumeCheck();
                    }
                }
            }
        }

        /// <summary>
        /// Prints the specified list of things to the console.
        /// </summary>
        private void Print(params dynamic[] list)
        {
            string output = string.Empty;

            for (int i = 0; i < list.Length; i++)
            {
                output += list[i] + " ";
            }

            Console.WriteLine(output);
        }

        /// <summary>
        /// Using the parameters found, packs the best solution found and
        /// reports to the console.
        /// </summary>
        private void Report()
        {
            _quit = false;

            switch (_bestVariant)
            {
                case 1:
                    _px = _container.Length; _py = _container.Height; _pz = _container.Width;
                    break;

                case 2:
                    _px = _container.Width; _py = _container.Height; _pz = _container.Length;
                    break;

                case 3:
                    _px = _container.Width; _py = _container.Length; _pz = _container.Height;
                    break;

                case 4:
                    _px = _container.Height; _py = _container.Length; _pz = _container.Width;
                    break;

                case 5:
                    _px = _container.Length; _py = _container.Width; _pz = _container.Height;
                    break;

                case 6:
                    _px = _container.Height; _py = _container.Width; _pz = _container.Length;
                    break;
            }

            _packingBest = true;

            _percentagePackedItemsByVolume = _bestVolume * 100 / _totalItemVolume;
            _percentageContainerUsed = _bestVolume * 100 / _totalContainerVolume;

            //Print("\n\n\nTOTAL NUMBER OF ITERATIONS DONE                       :", _iterationsCount);
            //Print("BEST SOLUTION FOUND AT ITERATION                      :", _bestIteration, "OF VARIANT", _bestVariant);
            //Print("TOTAL ITEMS TO PACK                                   :", _itemsToPackCount);
            //Print("PACKED ITEM COUNT                                     :", _bestPackedItemCount);
            //Print("TOTAL VOLUME OF ALL ITEMS                             :", _totalItemVolume);
            //Print("CONTAINER ID                                          :", _container.Id);
            //Print("CONTAINER VOLUME                                      :", _totalContainerVolume);
            //Print("BEST SOLUTION'S VOLUME UTILIZATION                    :", _bestVolume, "OUT OF", _totalContainerVolume);
            //Print("PERCENTAGE OF CONTAINER VOLUME USED                   :", _percentageContainerUsed);
            //Print("PERCENTAGE OF PACKED ITEMS (VOLUME)                   :", _percentagePackedItemsByVolume);
            //Print("WHILE CONTAINER ORIENTATION X - Y - Z                 :", _px, _py, _pz);
            //Print("---------------------------------------------------------------------------------------------");
            //Print("  NO: PACKSTA DIMEN-1  DMEN-2  DIMEN-3   COOR-X   COOR-Y   COOR-Z   PACKEDX  PACKEDY  PACKEDZ");
            //Print("---------------------------------------------------------------------------------------------");

            _layers.Clear();
            _layers.Add(new Layer { LayerEval = -1 });
            ListCanditLayers();
            _layers = _layers.OrderBy(l => l.LayerEval).ToList();
            _packedVolume = 0;
            _packedy = 0;
            _packing = true;
            _layerThickness = _layers[_bestIteration].LayerDim;
            _remainpy = _py;
            _remainpz = _pz;

            for (_x = 1; _x <= _itemsToPackCount; _x++)
            {
                _itemsToPack[_x].IsPacked = false;
            }

            do
            {
                _layerinlayer = 0;
                _layerDone = false;
                PackLayer();
                _packedy = _packedy + _layerThickness;
                _remainpy = _py - _packedy;

                if (_layerinlayer > 0.0001M)
                {
                    _prepackedy = _packedy;
                    _preremainpy = _remainpy;
                    _remainpy = _layerThickness - _prelayer;
                    _packedy = _packedy - _layerThickness + _prelayer;
                    _remainpz = _lilz;
                    _layerThickness = _layerinlayer;
                    _layerDone = false;
                    PackLayer();
                    _packedy = _prepackedy;
                    _remainpy = _preremainpy;
                    _remainpz = _pz;
                }

                if (!_quit)
                {
                    FindLayer(_remainpy);
                }
            } while (_packing && !_quit);

            //Console.WriteLine();
            //Console.WriteLine();
            //Print("*** LIST OF UNPACKED BOXES ***");
            _unpacked = true;

            for (_cboxi = 1; _cboxi <= _itemsToPackCount; _cboxi++)
            {
                if (!_itemsToPack[_cboxi].IsPacked)
                {
                    GraphUnpackedOut();
                }
            }

            _unpacked = false;

            //Console.WriteLine();

            //for (_n = 1; _n <= _itemsToPackCount; _n++)
            //{
            //    if (_itemsToPack[_n].IsPacked)
            //    {
            //        Print(_n, _itemsToPack[_n].Dim1, _itemsToPack[_n].Dim2, _itemsToPack[_n].Dim3, _itemsToPack[_n].CoordX, _itemsToPack[_n].CoordY, _itemsToPack[_n].CoordZ, _itemsToPack[_n].PackDimX, _itemsToPack[_n].PackDimY, _itemsToPack[_n].PackDimZ);
            //    }
            //}

            //Print("TOTAL NUMBER OF ITERATIONS DONE    : ", _iterationsCount);
            //Print("BEST SOLUTION FOUND AT             : ITERATION: " + _bestIteration + " OF VARIANT: ", _bestVariant);
            //Print("TOTAL NUMBER OF BOXES              : ", _itemsToPackCount);
            //Print("PACKED NUMBER OF BOXES             : ", _bestPackedItemCount);
            //Print("TOTAL VOLUME OF ALL BOXES          : ", _totalItemVolume);
            //Print("PALLET VOLUME                      : ", _totalContainerVolume);
            //Print("BEST SOLUTION'S VOLUME UTILIZATION : " + _bestVolume + " OUT OF ", _bestVolume, _totalContainerVolume);
            //Print("PERCENTAGE OF PALLET VOLUME USED   : ", _percentageContainerUsed);
            //Print("PERCENTAGE OF PACKEDBOXES (VOLUME) : ", _percentagePackedItemsByVolume);
            //Print("WHILE PALLET ORIENTATION           : X = " + _px + "; Y = " + _py + "; Z = " + _pz);
        }

        /// <summary>
        /// After packing of each item, the 100% packing condition is checked.
        /// </summary>
        private void VolumeCheck()
        {
            _itemsToPack[_cboxi].IsPacked = true;
            _itemsToPack[_cboxi].PackDimX = _cboxx;
            _itemsToPack[_cboxi].PackDimY = _cboxy;
            _itemsToPack[_cboxi].PackDimZ = _cboxz;
            _packedVolume = _packedVolume + _itemsToPack[_cboxi].Volume;
            _packedItemCount++;

            if (_packingBest)
            {
                GraphUnpackedOut();
                OutputBoxList();
            }
            else if (_packedVolume == _totalContainerVolume || _packedVolume == _totalItemVolume)
            {
                _packing = false;
                _hundredPercentPacked = true;
            }
        }

        /// <summary>
        /// A list that stores all the different lengths of all item dimensions.
        /// From the master's thesis:
        /// "Each Layerdim value in this array represents a different layer thickness
        /// value with which each iteration can start packing. Before starting iterations,
        /// all different lengths of all box dimensions along with evaluation values are
        /// stored in this array" (p. 3-6).
        /// </summary>
        private class Layer
        {
            /// <summary>
            /// Gets or sets the layer dimension value, representing a layer thickness.
            /// </summary>
            /// <value>
            /// The layer dimension value.
            /// </value>
            public decimal LayerDim { get; set; }

            /// <summary>
            /// Gets or sets the layer eval value, representing an evaluation weight
            /// value for the corresponding LayerDim value.
            /// </summary>
            /// <value>
            /// The layer eval value.
            /// </value>
            public decimal LayerEval { get; set; }
        }

        /// <summary>
        /// From the master's thesis:
        /// "The double linked list we use keeps the topology of the edge of the 
        /// current layer under construction. We keep the x and z coordinates of 
        /// each gap's right corner. The program looks at those gaps and tries to 
        /// fill them with boxes one at a time while trying to keep the edge of the
        /// layer even" (p. 3-7).
        /// </summary>
        private class ScrapPad
        {
            /// <summary>
            /// Gets or sets the x coordinate of the gap's right corner.
            /// </summary>
            /// <value>
            /// The x coordinate of the gap's right corner.
            /// </value>
            public decimal CumX { get; set; }

            /// <summary>
            /// Gets or sets the z coordinate of the gap's right corner.
            /// </summary>
            /// <value>
            /// The z coordinate of the gap's right corner.
            /// </value>
            public decimal CumZ { get; set; }

            /// <summary>
            /// Gets or sets the following entry.
            /// </summary>
            /// <value>
            /// The following entry.
            /// </value>
            public ScrapPad Post { get; set; }

            /// <summary>
            /// Gets or sets the previous entry.
            /// </summary>
            /// <value>
            /// The previous entry.
            /// </value>
            public ScrapPad Pre { get; set; }
        }
    }
}
