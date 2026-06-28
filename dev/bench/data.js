window.BENCHMARK_DATA = {
  "lastUpdate": 1782664109308,
  "repoUrl": "https://github.com/Chris-Wolfgang/ETL-Test-Kit",
  "entries": {
    "BenchmarkDotNet": [
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "1c68473a35c4c2ab3e2e7be7c37c5b6944f82152",
          "message": "Merge pull request #188 from Chris-Wolfgang/fix/benchmarks-workflow-to-main\n\nci: add benchmarks.yaml workflow (protected-file split for 0.9.0)",
          "timestamp": "2026-06-25T13:50:22-04:00",
          "tree_id": "157ac8de848eeb8a7787f27cea509c7cb88f6559",
          "url": "https://github.com/Chris-Wolfgang/ETL-Test-Kit/commit/1c68473a35c4c2ab3e2e7be7c37c5b6944f82152"
        },
        "date": 1782409941828,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 1000)",
            "value": 28602.093271891277,
            "unit": "ns",
            "range": "± 2054.533657278846"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 10000)",
            "value": 299877.27327473956,
            "unit": "ns",
            "range": "± 1301.1323141941248"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 100000)",
            "value": 2553518.9088541665,
            "unit": "ns",
            "range": "± 62898.0729089019"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 1000)",
            "value": 24545.789611816406,
            "unit": "ns",
            "range": "± 67.26719027627661"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 10000)",
            "value": 244605.2939453125,
            "unit": "ns",
            "range": "± 347.1632103578914"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 100000)",
            "value": 2395511.2877604165,
            "unit": "ns",
            "range": "± 4801.6891041157405"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 1000)",
            "value": 43454.49100748698,
            "unit": "ns",
            "range": "± 67.58489811216045"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 10000)",
            "value": 422133.60074869794,
            "unit": "ns",
            "range": "± 1257.0108542852186"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 100000)",
            "value": 4240075.78515625,
            "unit": "ns",
            "range": "± 5524.870633742684"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "969bca1fd49fbea3c8d01e9595c2a93c717d2dfa",
          "message": "Merge pull request #187 from Chris-Wolfgang/vNext\n\nRelease 0.9.0",
          "timestamp": "2026-06-25T16:37:55-04:00",
          "tree_id": "eb0e9ac0f5fbc03190974c7dd4f0e253d53ccb6c",
          "url": "https://github.com/Chris-Wolfgang/ETL-Test-Kit/commit/969bca1fd49fbea3c8d01e9595c2a93c717d2dfa"
        },
        "date": 1782419998130,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 1000)",
            "value": 60162.58600870768,
            "unit": "ns",
            "range": "± 4521.013144155842"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 10000)",
            "value": 382204.22347005206,
            "unit": "ns",
            "range": "± 7952.588181872598"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 100000)",
            "value": 3533113.7317708335,
            "unit": "ns",
            "range": "± 24732.64467531271"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 1000)",
            "value": 16067.614888509115,
            "unit": "ns",
            "range": "± 47.89371715521378"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 10000)",
            "value": 156155.81392415366,
            "unit": "ns",
            "range": "± 71.25990794974734"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 100000)",
            "value": 1576029.4772135417,
            "unit": "ns",
            "range": "± 7745.448634190598"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 1000)",
            "value": 50092.98585001627,
            "unit": "ns",
            "range": "± 91.04833471051582"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 10000)",
            "value": 497633.126953125,
            "unit": "ns",
            "range": "± 1280.1743025838593"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 100000)",
            "value": 4963530.372395833,
            "unit": "ns",
            "range": "± 19841.55397209163"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "210299580+Chris-Wolfgang@users.noreply.github.com",
            "name": "Chris Wolfgang",
            "username": "Chris-Wolfgang"
          },
          "committer": {
            "email": "noreply@github.com",
            "name": "GitHub",
            "username": "web-flow"
          },
          "distinct": true,
          "id": "48381bd63b7d618d8ba9bf607143f25411cf2dca",
          "message": "Merge pull request #191 from Chris-Wolfgang/dependabot/github_actions/github-actions-640176b5ab\n\nbuild(deps): bump actions/checkout from 6 to 7 in the github-actions group across 1 directory",
          "timestamp": "2026-06-28T12:26:31-04:00",
          "tree_id": "0acae67fe8c93c77e3d7a685b7020b843801f36d",
          "url": "https://github.com/Chris-Wolfgang/ETL-Test-Kit/commit/48381bd63b7d618d8ba9bf607143f25411cf2dca"
        },
        "date": 1782664106849,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 1000)",
            "value": 61831.10775756836,
            "unit": "ns",
            "range": "± 559.617403089337"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 10000)",
            "value": 378524.52880859375,
            "unit": "ns",
            "range": "± 4978.615913858545"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 100000)",
            "value": 3842271.40234375,
            "unit": "ns",
            "range": "± 31861.597211235552"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 1000)",
            "value": 16245.524169921875,
            "unit": "ns",
            "range": "± 66.9652449392168"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 10000)",
            "value": 156378.75919596353,
            "unit": "ns",
            "range": "± 62.06753428495238"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 100000)",
            "value": 1575906.0859375,
            "unit": "ns",
            "range": "± 12409.53181156354"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 1000)",
            "value": 49133.239613850914,
            "unit": "ns",
            "range": "± 183.9088939606873"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 10000)",
            "value": 496285.8277994792,
            "unit": "ns",
            "range": "± 370.8190262412704"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 100000)",
            "value": 5051961.604166667,
            "unit": "ns",
            "range": "± 3633.77299736382"
          }
        ]
      }
    ]
  }
}