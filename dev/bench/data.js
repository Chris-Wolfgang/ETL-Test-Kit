window.BENCHMARK_DATA = {
  "lastUpdate": 1782409944880,
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
      }
    ]
  }
}