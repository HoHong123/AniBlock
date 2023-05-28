// ----------------------------------------------
// --- Python
// --- Author         : Ahmet Ozlu
// --- Mail           : ahmetozlu93@gmail.com
// --- Date           : 31st December 2017 - new year eve :)
// ----------------------------------------------
// --- Convert Python to C#
// --- NAHUN KIM
// ----------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

public class KNN_Classifier : MonoBehaviour {
    float CalculateEuclidenDistance(Feature test_feauture, Feature training_feature)
    {
        float dist = 0.0f;
        dist = Mathf.Sqrt(Mathf.Pow(test_feauture.R - training_feature.R, 2)) + Mathf.Sqrt(Mathf.Pow(test_feauture.G - training_feature.G, 2)) + Mathf.Sqrt(Mathf.Pow(test_feauture.B - training_feature.B, 2));
        return dist;
    }

    List<Distance> KNearestNeighbors(List<Feature> training_feature_list, Feature test_feature, int k)
    {
        float dist;
        Distance distance = null;
        List<Distance> distances = new List<Distance>();
        List<Distance> neighbors = new List<Distance>();
        for (int i = 0; i < training_feature_list.Count; i++)
        {
            dist = CalculateEuclidenDistance(test_feature, training_feature_list[i]);
            distance = new Distance(training_feature_list[i], dist);
            distances.Add(distance);
        }
        // sort
        // https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.sort?view=netframework-4.8
        distances.Sort(delegate (Distance distnace1, Distance distnace2)
        {
            if (distnace2 == null) return 1;
            else return distnace1.Dist.CompareTo(distnace2.Dist);
        });

        for (int i = 0; i < k; i++)
        {
            if(distances.Count > 0){
                neighbors.Add(distances[i]);
            }
        }

        return neighbors;
    }

    string ResponseOfNeighbors(List<Distance> neighbors)
    {
        int[] all_possible_neighbors = new int[6];
        for (int i = 0; i < neighbors.Count; i++)
        {
            switch (neighbors[i].Feature.Label)
            {
                case "red":
                    all_possible_neighbors[0]++;
                    break;
                case "orange":
                    all_possible_neighbors[1]++;
                    break;
                case "yellow":
                    all_possible_neighbors[2]++;
                    break;
                case "green":
                    all_possible_neighbors[3]++;
                    break;
                case "blue":
                    all_possible_neighbors[4]++;
                    break;
                case "brown":
                    all_possible_neighbors[5]++;
                    break;
            }
        }
        int maxval = all_possible_neighbors.Max();
        int maxIdx = Array.IndexOf(all_possible_neighbors, maxval);
        switch (maxIdx)
        {
            case 0:
                return "red";
            case 1:
                return "orange";
            case 2:
                return "yellow";
            case 3:
                return "green";
            case 4:
                return "blue";
            case 5:
                return "brown";
            default:
                return null;
        }
    }

    void LoadDataset(string training_data_name, string test_data_name, List<Feature> trainig_feature_list, List<Feature> test_feature_list)
    {
        string[] text = File.ReadAllLines(PATH.GetOnResources(training_data_name + ".data"));
        Feature feature;
        string[] sub_text;
        for (int i = 0; i < text.Length; i++)
        {
            sub_text = text[i].Split(',');
            feature = new Feature(int.Parse(sub_text[0]), int.Parse(sub_text[1]), int.Parse(sub_text[2]), sub_text[3]);
            trainig_feature_list.Add(feature);
        }

        text = File.ReadAllLines(PATH.GetOnResources(test_data_name + ".data"));
        for (int i = 0; i < text.Length; i++)
        {
            sub_text = text[i].Split(',');
            feature = new Feature(int.Parse(sub_text[0]), int.Parse(sub_text[1]), int.Parse(sub_text[2]));
            test_feature_list.Add(feature);
        }
    }

    public void LoadTrainingDataset(string training_data_name, List<Feature> trainig_feature_list)
    {
        string[] text = File.ReadAllLines(PATH.GetOnResources(training_data_name));
        Feature feature;
        string[] sub_text;
        for (int i = 0; i < text.Length; i++)
        {
            sub_text = text[i].Split(',');
            feature = new Feature(int.Parse(sub_text[0]), int.Parse(sub_text[1]), int.Parse(sub_text[2]), sub_text[3]);
            trainig_feature_list.Add(feature);
        }
    }

    public List<string> KNN_Main(string training_data_name, string test_data_name)
    {
        List<Feature> training_feature_list = new List<Feature>();
        List<Feature> test_feature_list = new List<Feature>();
        List<Feature> classifier_prediction = new List<Feature>();
        List<Distance> neighbors = null;
        List<string> results = new List<string>();
        int k = 3;
        string result = null;
        LoadDataset(training_data_name, test_data_name, training_feature_list, test_feature_list);
        for (int i = 0; i < test_feature_list.Count; i++)
        {
            neighbors = KNearestNeighbors(training_feature_list, test_feature_list[i], k);
            result = ResponseOfNeighbors(neighbors);
            results.Add(result);
        }
        return results;
    }

    public List<string> KNN_Main(List<Feature> training_feature_list, List<Feature> test_feature_list)
    {
        // List<Feature> training_feature_list = new List<Feature>();
        // List<Feature> test_feature_list = new List<Feature>();
        List<Feature> classifier_prediction = new List<Feature>();
        List<Distance> neighbors = null;
        List<string> results = new List<string>();
        int k = 3;
        string result = null;
        // LoadDataset(training_data_name, test_data_name, training_feature_list, test_feature_list);
        for (int i = 0; i < test_feature_list.Count; i++)
        {
            neighbors = KNearestNeighbors(training_feature_list, test_feature_list[i], k);
            result = ResponseOfNeighbors(neighbors);
            results.Add(result);
        }
        return results;
    }
}

public class Feature
{
    public Feature(int R = 0, int G = 0, int B = 0, string Label = null, int FeatureNum = 3)
    {
        this.R = R;
        this.G = G;
        this.B = B;
        this.Label = Label;
        this.FeatureNum = FeatureNum;
    }
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }
    public string Label { get; set; }
    public int FeatureNum { get; set; }
}

public class Distance
{
    public Distance(Feature Feature, float Dist)
    {
        this.Feature = Feature;
        this.Dist = Dist;
    }
    public Feature Feature { get; set; }
    public float Dist { get; set; }
}