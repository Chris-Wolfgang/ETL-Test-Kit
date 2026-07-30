window.BENCHMARK_DATA = {
  "lastUpdate": 1785413014800,
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
          "id": "c9e2a37a00fb1ae6885e306ecb0c18eb6b1ddcc1",
          "message": "Merge pull request #246 from Chris-Wolfgang/vNext\n\nrelease: 0.10.1 (thorough-review maintenance tier + Abstractions 0.16.0)",
          "timestamp": "2026-07-26T20:21:11-04:00",
          "tree_id": "3caf71bb9e6a1ddde30b4276817ed698c4173ab1",
          "url": "https://github.com/Chris-Wolfgang/ETL-Test-Kit/commit/c9e2a37a00fb1ae6885e306ecb0c18eb6b1ddcc1"
        },
        "date": 1785111781537,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 1000)",
            "value": 59506.411529541016,
            "unit": "ns",
            "range": "± 2011.1222214406387"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 10000)",
            "value": 387334.3291829427,
            "unit": "ns",
            "range": "± 11586.998105205032"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 100000)",
            "value": 3647736.9869791665,
            "unit": "ns",
            "range": "± 21093.16730405295"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 1000)",
            "value": 15862.945643107096,
            "unit": "ns",
            "range": "± 58.268393412868235"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 10000)",
            "value": 157491.73111979166,
            "unit": "ns",
            "range": "± 2796.9676089183126"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 100000)",
            "value": 1569172.1432291667,
            "unit": "ns",
            "range": "± 7251.174462366801"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 1000)",
            "value": 50078.76055908203,
            "unit": "ns",
            "range": "± 132.88683228909932"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 10000)",
            "value": 497642.9892578125,
            "unit": "ns",
            "range": "± 813.0597085481849"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 100000)",
            "value": 5678513.716145833,
            "unit": "ns",
            "range": "± 2769.2074862583077"
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
          "id": "d4b58df2fded9933b8125fb3851b70c5abd1a2d0",
          "message": "Merge pull request #254 from Chris-Wolfgang/vNext\n\nRelease 0.11.0 — Abstractions 0.18.1 adoption",
          "timestamp": "2026-07-27T21:12:16-04:00",
          "tree_id": "a7c47fd9036e12d917a44cfcf717a83f06cfb7ea",
          "url": "https://github.com/Chris-Wolfgang/ETL-Test-Kit/commit/d4b58df2fded9933b8125fb3851b70c5abd1a2d0"
        },
        "date": 1785201241944,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 1000)",
            "value": 60221.92825317383,
            "unit": "ns",
            "range": "± 1088.1399742958838"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 10000)",
            "value": 377119.80029296875,
            "unit": "ns",
            "range": "± 3420.4666841625926"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 100000)",
            "value": 3673123.2734375,
            "unit": "ns",
            "range": "± 11129.225872134379"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 1000)",
            "value": 15879.977391560873,
            "unit": "ns",
            "range": "± 48.946962354356224"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 10000)",
            "value": 157642.3046875,
            "unit": "ns",
            "range": "± 95.18189149058384"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 100000)",
            "value": 1575740.5240885417,
            "unit": "ns",
            "range": "± 5252.659974623049"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 1000)",
            "value": 51029.40846761068,
            "unit": "ns",
            "range": "± 168.95334344649223"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 10000)",
            "value": 505993.2034505208,
            "unit": "ns",
            "range": "± 371.32295900177434"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 100000)",
            "value": 4945502.0078125,
            "unit": "ns",
            "range": "± 8650.652836547893"
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
          "id": "f5ad3a5a3b01e6badbaf20becd1a2e53633dcdb4",
          "message": "Merge pull request #275 from Chris-Wolfgang/vNext\n\nRelease 0.12.0",
          "timestamp": "2026-07-28T21:49:45-04:00",
          "tree_id": "62ec6a028c3cb0501c25f2073857cb1643957367",
          "url": "https://github.com/Chris-Wolfgang/ETL-Test-Kit/commit/f5ad3a5a3b01e6badbaf20becd1a2e53633dcdb4"
        },
        "date": 1785289893816,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 1000)",
            "value": 71245.51487223308,
            "unit": "ns",
            "range": "± 1891.0312803980291"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 10000)",
            "value": 460156.55126953125,
            "unit": "ns",
            "range": "± 933.1590739090213"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 100000)",
            "value": 4415679.20703125,
            "unit": "ns",
            "range": "± 11267.404027169461"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 1000)",
            "value": 24373.30303955078,
            "unit": "ns",
            "range": "± 39.411914609508344"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 10000)",
            "value": 246876.98673502603,
            "unit": "ns",
            "range": "± 674.1989731117567"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 100000)",
            "value": 2388757.5130208335,
            "unit": "ns",
            "range": "± 8525.381532435671"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 1000)",
            "value": 61804.43253580729,
            "unit": "ns",
            "range": "± 202.3579647706498"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 10000)",
            "value": 602817.5755208334,
            "unit": "ns",
            "range": "± 1055.4606625380889"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 100000)",
            "value": 6140403.7421875,
            "unit": "ns",
            "range": "± 1760.86302637549"
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
          "id": "8fb17b53b7441ad605a4ddf393fa2836b6cdf533",
          "message": "Merge pull request #276 from Chris-Wolfgang/vNext-0.20.0\n\nRelease 0.13.0 — Abstractions 0.20 adoption: retry / clock / middleware / aggregate-errors + scenario harness",
          "timestamp": "2026-07-30T08:01:45-04:00",
          "tree_id": "de98e197496fb4d7da7369c5b9367e2c9119245d",
          "url": "https://github.com/Chris-Wolfgang/ETL-Test-Kit/commit/8fb17b53b7441ad605a4ddf393fa2836b6cdf533"
        },
        "date": 1785413011806,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 1000)",
            "value": 62222.972106933594,
            "unit": "ns",
            "range": "± 1226.1394103544988"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 10000)",
            "value": 414787.2292480469,
            "unit": "ns",
            "range": "± 628.5145799966474"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.ExtractorBenchmarks.Extract(ItemCount: 100000)",
            "value": 3944541.5364583335,
            "unit": "ns",
            "range": "± 4045.5777045311156"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 1000)",
            "value": 21260.482467651367,
            "unit": "ns",
            "range": "± 179.54547733399266"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 10000)",
            "value": 212285.14591471353,
            "unit": "ns",
            "range": "± 831.2432143653004"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.LoaderBenchmarks.Load(ItemCount: 100000)",
            "value": 2101376.0143229165,
            "unit": "ns",
            "range": "± 867.5861856644949"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 1000)",
            "value": 55203.32719930013,
            "unit": "ns",
            "range": "± 107.77204305282"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 10000)",
            "value": 541339.5328776041,
            "unit": "ns",
            "range": "± 1365.3112766756617"
          },
          {
            "name": "Wolfgang.Etl.TestKit.Benchmarks.TransformerBenchmarks.Transform(ItemCount: 100000)",
            "value": 5453982.190104167,
            "unit": "ns",
            "range": "± 3892.8954658365155"
          }
        ]
      }
    ]
  }
}