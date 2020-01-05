import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"
import { SeasonContext } from "./SeasonContext"

export default class TopStatsQuery extends Component {
  static contextType = SeasonContext;
  render() {
    const { children } = this.props
    const season = this.context;

    return (
      <Query query={GET_TOPSTATS} variables={{ season }}>
        {result => children(result)}
      </Query>
    )
  }
}

const GET_TOPSTATS = gql`
query allStats($season: Int) {
  allCharacters(condition: {season: $season}) {
    nodes {
      nodeId,
      id,
      name,
      killmailsByAttackerId {
        totalCount
      }
    }
  }
  allGuilds(condition: {season: $season}) {
    nodes {
      nodeId,
      id,
      name,
      killmailsByAttackerGuildId {
        totalCount
      }
    }
  }
  allGuildRankedKillDeaths(first: 50, condition: {season: $season}, orderBy: RANKED_KILLS_DESC) {
    nodes {
      id
      name
      rankedKills
      rankedDeaths
    }
  }
  allCharacterRankedKillDeaths(first: 50, condition: {season: $season}, orderBy: RANKED_KILLS_DESC) {
    nodes {
      id
      name
      rankedKills
      rankedDeaths
    }
  }
}
`;
