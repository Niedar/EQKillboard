import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"
import { SeasonContext } from "./SeasonContext"

export default class TopStatsQuery extends Component {
  static contextType = SeasonContext;
  render() {
    const { children, onCompleted } = this.props
    const season = this.context;

    return (
      <Query query={GET_TOPSTATS} variables={{ season }} onCompleted={onCompleted}>
        {result => children(result)}
      </Query>
    )
  }
}

const GET_TOPSTATS = gql`
query allStats($season: Int) {
  allCharacters(condition: {season: $season, isNpc: false}) {
    nodes {
      nodeId
      id
      name
      classId
      killmailInvolvedsByAttackerId {
        totalCount
      }
    }
  }
  allGuilds(condition: {season: $season}) {
    nodes {
      nodeId
      id
      name
      kills
    }
  }
  allGuildRankedKillDeathInvolveds(condition: {season: $season}, orderBy: RANKED_KILLS_DESC) {
    nodes {
      id
      name
      rankedKills
      rankedDeaths
    }
  }
  allCharacterRankedKillDeathInvolveds(condition: {season: $season, isNpc: false}, orderBy: RANKED_KILLS_DESC) {
    nodes {
      id
      name
      classId
      rankedKills
      rankedDeaths
    }
  }
  allClasses(orderBy: NAME_ASC) {
    nodes {
      id
      name
    }
  }
}
`;
